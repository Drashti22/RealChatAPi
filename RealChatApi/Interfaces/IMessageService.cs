using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;

namespace RealChatApi.Interfaces
{
    public interface IMessageService
    {
         Task<IActionResult> SendMessage(SendMessageRequestDto requestdto, string CurrentUser);
    }
}
