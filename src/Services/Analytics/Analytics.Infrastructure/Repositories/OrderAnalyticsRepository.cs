using Analytics.Domain.Entities;
using Analytics.Domain.Interfaces;
using Analytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

namespace Analytics.Infrastructure.Repositories;

public class OrderAnalyticsRepository : IOrderAnalyticsRepository
{
    private readonly AnalyticsDbContext _context;

    public OrderAnalyticsRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OrderAnalyticsEntry entry)
    {
        _context.OrderAnalyticsEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OrdersPerYearDto>> GetOrdersPerYearByCustomerAsync(Guid customerId)
    {
        return await _context.OrderAnalyticsEntries
            .Where(e => e.CustomerId == customerId)
            .GroupBy(e => e.Year)
            .Select(g => new OrdersPerYearDto(g.Key, g.Count()))
            .OrderBy(x => x.Year)
            .ToListAsync();
    }
}
