using Analytics.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IOrderAnalyticsRepository _repository;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IOrderAnalyticsRepository repository, ILogger<AnalyticsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Returns orders-per-year for a given customer (bar graph data).
    /// </summary>
    [HttpGet("orders-per-year/{customerId:guid}")]
    public async Task<IActionResult> GetOrdersPerYear(Guid customerId)
    {
        var data = await _repository.GetOrdersPerYearByCustomerAsync(customerId);
        return Ok(data);
    }

    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new { service = "Analytics", status = "running" });
    }
}
