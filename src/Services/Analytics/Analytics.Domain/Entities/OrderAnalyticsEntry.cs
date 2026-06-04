namespace Analytics.Domain.Entities;

public class OrderAnalyticsEntry
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public int Year { get; set; }
    public DateTime PlacedAt { get; set; }
}
