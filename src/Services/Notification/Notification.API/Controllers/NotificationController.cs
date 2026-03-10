using Microsoft.AspNetCore.Mvc;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(ILogger<NotificationController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        // TODO: Implement — migrate logic from monolith's NotificationController
        return Ok(new { service = "Notification", status = "scaffold" });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        // TODO: Implement — migrate logic from monolith
        return Ok(new { service = "Notification", id });
    }
}
