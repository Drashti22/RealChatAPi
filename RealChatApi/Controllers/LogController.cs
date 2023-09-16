using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealChatApi.Interfaces;

namespace RealChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {

        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }


        [HttpGet("log")]
        public async Task<IActionResult> GetLogs( [FromQuery] string startTime = null, [FromQuery] string endTime = null)
        {
            return await _logService.getLogs( startTime, endTime);
        }
    }
}
