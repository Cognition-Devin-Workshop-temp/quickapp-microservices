using System.Text;
using System.Text.Json;
using Analytics.Domain.Entities;
using Analytics.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;

namespace Analytics.Infrastructure.Messaging;

public class OrderPlacedEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderPlacedEventConsumer> _logger;
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;

    private IConnection? _connection;
    private IChannel? _channel;

    public OrderPlacedEventConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrderPlacedEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _hostName = configuration["RabbitMQ:Host"] ?? "rabbitmq";
        _userName = configuration["RabbitMQ:Username"] ?? "guest";
        _password = configuration["RabbitMQ:Password"] ?? "guest";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitializeRabbitMqAsync(stoppingToken);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (_channel is null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(message);

                if (orderEvent is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IOrderAnalyticsRepository>();

                    var entry = new OrderAnalyticsEntry
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderEvent.OrderId,
                        CustomerId = orderEvent.CustomerId,
                        TotalAmount = orderEvent.TotalAmount,
                        Year = orderEvent.PlacedAt.Year,
                        PlacedAt = orderEvent.PlacedAt
                    };

                    await repository.AddAsync(entry);
                    _logger.LogInformation("Processed OrderPlacedEvent for Order {OrderId}", orderEvent.OrderId);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderPlacedEvent");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "analytics-order-placed",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeRabbitMqAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = _userName,
            Password = _password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: "order-placed",
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: "analytics-order-placed",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: "analytics-order-placed",
            exchange: "order-placed",
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Connected to RabbitMQ and listening for OrderPlacedEvent");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);
        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);
    }
}
