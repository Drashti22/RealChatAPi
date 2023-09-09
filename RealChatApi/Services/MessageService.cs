using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using RealChatApi.Models;
using RealChatApi.Repositories;
using System.Security.Claims;

namespace RealChatApi.Services
{
    public class MessageService: IMessageService
    {
        private readonly ApplicationDbContext _authContext;
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MessageService(ApplicationDbContext authContext, IMessageRepository messageRepository, IHttpContextAccessor httpContextAccessor)
        {
            _authContext = authContext;
            _messageRepository = messageRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> SendMessage(SendMessageRequestDto requestdto, string CurrentUser)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserID = new ApplicationUser { Id = userId };
            var sender = await _messageRepository.FindUserByEmailAsync(currentUserID);
            
            
            if (sender == null)
            {
                throw new InvalidOperationException("Unauthorized");
            }
            if (requestdto == null || string.IsNullOrWhiteSpace(requestdto.Content)) 
            {
                return new BadRequestObjectResult(new { Message = "Message content is required" });
            }
            var receiver = await _messageRepository.FindReceiver(requestdto.ReceiverId);
            if( receiver == null)
            {
                return new NotFoundObjectResult(new { Message = "Receiver Not Found" });
            }
            if (currentUserID == receiver)
            {
                return new BadRequestObjectResult(new { Message = "Bad Request" });
            }
                var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = requestdto.Content,
                Timestamp = DateTime.Now
            };
            message = await _messageRepository.SendMessageAsync(message);

            var response = new SendMessageResponseDTO
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            };
            return new OkObjectResult(response);
        }           

    }
}
