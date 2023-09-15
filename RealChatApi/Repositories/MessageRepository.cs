using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;
using System.Security.Claims;

namespace RealChatApi.Repositories
{
    public class MessageRepository : IMessageRepository
    {

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MessageRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApplicationUser> FindUserByEmailAsync(ApplicationUser CurrentUser)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            return user;
        }
        public async Task<ApplicationUser> FindReceiver(string ReceiverId)
        {
            var receiver = await _context.Users.FindAsync(ReceiverId);
            return receiver;
        }
        public async Task<Message> SendMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Message> FindMessage(int messageId)
        {
            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                return message;
            }
            return null;
        }

        public async Task<Message> EditMessage(Message message)
        {
            await _context.SaveChangesAsync();
            return message;
        }
        public async Task<Message> DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return message;
        }
        public async Task<IQueryable<Message>> QueryMessage(string userId1, string userId2)
        {
           return _context.Messages
                .Where(m => (m.SenderId == (userId1)) && m.ReceiverId == userId2 ||
                             (m.SenderId == userId2 && m.ReceiverId == (userId1)));  
        }

        public async Task<bool> userIdExists(string userId)
        {
           return _context.Users.Any(r => r.Id == userId);
        }

        public async Task<List<object>> SearchConversations(string userId1, string userId2, string query)
        {
            var searchResults = _context.Messages
       .Where(m => (m.SenderId == userId1 || m.ReceiverId == userId1) && (m.SenderId == userId2 || m.ReceiverId == userId2) && m.Content.Contains(query))
       .Select(m => new
       {
           id = m.Id,
           senderId = m.SenderId,
           receiverId = m.ReceiverId,
           content = m.Content,
           timestamp = m.Timestamp,
       })
       .ToList<object>();

            return searchResults;
        }
    }
}
