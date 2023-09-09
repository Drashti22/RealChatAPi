using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;
using System.Security.Claims;

namespace RealChatApi.Repositories
{
    public class MessageRepository: IMessageRepository
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
    }
}
