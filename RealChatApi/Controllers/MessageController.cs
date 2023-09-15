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
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto requestdto)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _messageService.SendMessage(requestdto, currentUserId);
        }

        [HttpPut]
        [Route("messages/messageId")]
        public async Task<IActionResult> EditMessage([FromBody] EditMessageRequestDto requestdto, int messageId)
        {
            return await _messageService.EditMessage(requestdto, messageId);
        }

        [HttpDelete]
        [Route("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            return await _messageService.DeleteMessage(messageId);
        }


        [HttpGet("messages")]
        public async Task<IActionResult> GetConversationHistory(string userId, string? before = null, int count = 20, string sort = "asc")
        {
            return await _messageService.GetConversationHistory(userId, before, count, sort);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchConversation([FromQuery]string keyword)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { Message = "Invalid query parameter" });
            }
            return await _messageService.SearchConversation( currentUserId, keyword);

        }
    }
}
