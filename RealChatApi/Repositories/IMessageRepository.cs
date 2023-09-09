using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface IMessageRepository
    {
         Task<ApplicationUser> FindUserByEmailAsync(ApplicationUser CurrentUser);

         Task<ApplicationUser> FindReceiver(string receiverId);

         Task<Message> SendMessageAsync(Message message);
    }
}
