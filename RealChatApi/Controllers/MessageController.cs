using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using RealChatApi.Services;
using System.Security.Claims;

namespace RealChatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

   
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public MessageController(IMessageService messageService, IHttpContextAccessor httpContextAccessor) 
        {
            _messageService = messageService;
             _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody]SendMessageRequestDto requestdto)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _messageService.SendMessage(requestdto, currentUserId);
        }
    }
}
