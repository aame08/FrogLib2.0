using Microsoft.AspNetCore.Mvc;

namespace FrogLib.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected ActionResult HandleException(Exception ex)
        {
            return StatusCode(500, new { error = $"Внутренняя ошибка: {ex.Message}" });
        }
    }
}
