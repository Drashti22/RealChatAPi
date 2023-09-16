using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using RealChatApi.Models;
using RealChatApi.Repositories;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RealChatApi.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _authContext;
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly static connection<string> _connections = new connection<string>();
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageService(ApplicationDbContext authContext, IMessageRepository messageRepository, IHttpContextAccessor httpContextAccessor, IHubContext<ChatHub> hubContext)
        {
            _authContext = authContext;
            _messageRepository = messageRepository;
            _httpContextAccessor = httpContextAccessor;
            _hubContext = hubContext;
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
            if (receiver == null)
            {
                return new NotFoundObjectResult(new { Message = "Receiver Not Found" });
            }
            if (currentUserID.Id == receiver.Id)
            {
                return new BadRequestObjectResult(new { Message = "Can not send a message to yourself." });
            }
            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = requestdto.Content,
                Timestamp = DateTime.Now
            };
            message = await _messageRepository.SendMessageAsync(message);
            foreach (var connectionId in _connections.GetConnections(message.ReceiverId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("BroadCast", message);
            }

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

        public async Task<IActionResult> EditMessage(EditMessageRequestDto requestdto, int messageId)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;



            if (requestdto == null || string.IsNullOrWhiteSpace(requestdto.Content))
            {
                return new BadRequestObjectResult(new { Message = "Updated message content is required" });
            }

            var message = await _messageRepository.FindMessage(messageId);
            if (message == null)
            {
                return new NotFoundObjectResult(new { Message = "Message not found" });
            }
            if (message.SenderId != userId)
            {
                throw new InvalidOperationException("Unauthorized");
            }
            message.Content = requestdto.Content;
            await _messageRepository.EditMessage(message);
            return new OkObjectResult(new { Message = "Message Edited Successfully" });
        }

        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = await _messageRepository.FindMessage(messageId);
            if (message == null)
            {
                return new NotFoundObjectResult(new { Message = "Message not found" });
            }
            if (message.SenderId != userId)
            {
                throw new InvalidOperationException("Unauthorized");
            }
            await _messageRepository.DeleteMessage(message);
            return new OkObjectResult(new { Message = "Message Deleted Successfully" });
        }

        public async Task<IActionResult> GetConversationHistory(string userId, string? before = null, int count = 20, string sort = "asc")
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var sender = await _messageRepository.FindUserByEmailAsync(currentUserID);

            if (authenticatedUserId == null)
            {
                return new UnauthorizedResult();
            }
            var currentUserID = new ApplicationUser { Id = authenticatedUserId };

            var userIdexists = await _messageRepository.userIdExists(userId);
            if (!userIdexists)
            {
                return new NotFoundObjectResult(new { Message = "User does not exist." });
            }

            var messageQuery = await _messageRepository.QueryMessage(userId, authenticatedUserId);
            if (!string.IsNullOrEmpty(before) && DateTime.TryParse(before, out var beforeTime))
            {
                if (sort == "asc")
                {
                    messageQuery = messageQuery
                        .Where(m => m.Timestamp < beforeTime)
                        .OrderBy(m => m.Timestamp);
                }
                else if (sort == "desc")
                {
                    messageQuery = messageQuery
                        .Where(m => m.Timestamp < beforeTime)
                        .OrderByDescending(m => m.Timestamp);
                }
            }
            else
            {
                // No 'before' timestamp provided, apply sorting based on the 'sort' parameter
                messageQuery = sort == "asc"
                    ? messageQuery.OrderByDescending(m => m.Timestamp)
                    : messageQuery.OrderBy(m => m.Timestamp);
            }

            var messages = messageQuery
                .Take(count)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp,
                }).ToList();



            if (messages.Count == 0)
            {
                return new OkObjectResult(new List<object>());
            }
            return new OkObjectResult(new { messages });

        }

        public async Task<IActionResult> SearchConversation(string userId, string keyword)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (authenticatedUserId == null)
            {
                return new UnauthorizedResult();
            }
            var currentUserID = new ApplicationUser { Id = authenticatedUserId };

            var userIdExists = await _messageRepository.userIdExists(userId);
            if (!userIdExists)
            {
                return new NotFoundObjectResult(new { Message = "User does not exist." });
            }
            var searchResults = await _messageRepository.SearchConversations(userId, authenticatedUserId, keyword);

            if (searchResults == null)
            {
                return new OkObjectResult(new List<object>());
            }

            return new OkObjectResult(new { messages = searchResults });
        }

        
    }
}
