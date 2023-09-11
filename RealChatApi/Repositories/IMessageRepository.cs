using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface IMessageRepository
    {
        Task<ApplicationUser> FindUserByEmailAsync(ApplicationUser CurrentUser);

        Task<ApplicationUser> FindReceiver(string receiverId);

        Task<Message> SendMessageAsync(Message message);

        Task<Message> FindMessage(int messageId);

        Task<Message> EditMessage(Message message);

        Task<Message> DeleteMessage(Message message);

        Task<IQueryable<Message>> QueryMessage(string userId1, string userId2);

        Task<bool> userIdExists(string userId);

        Task<List<object>> SearchConversations(string userId1, string userId2, string query);
    }
}
