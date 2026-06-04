using Analytics.Domain.Entities;
using Shared.Contracts.DTOs;

namespace Analytics.Domain.Interfaces;

public interface IOrderAnalyticsRepository
{
    Task AddAsync(OrderAnalyticsEntry entry);
    Task<bool> ExistsByOrderIdAsync(Guid orderId);
    Task<List<OrdersPerYearDto>> GetOrdersPerYearByCustomerAsync(Guid customerId);
}
