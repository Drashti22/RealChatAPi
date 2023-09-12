using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;

namespace RealChatApi.Interfaces
{
    public interface IMessageService
    {
         Task<IActionResult> SendMessage(SendMessageRequestDto requestdto, string CurrentUser);

        Task<IActionResult> EditMessage(EditMessageRequestDto requestdto, int messageId);

        Task<IActionResult> DeleteMessage(int messageId);

        Task<IActionResult> GetConversationHistory(string userId, string? before = null, int count = 20, string sort = "asc");

        Task<IActionResult> SearchConversation(string userId, string keyword);
    }
}
