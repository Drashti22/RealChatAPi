using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface ILogRepository
    {
        Task<IActionResult> getLogs(DateTime? parsedStartTime, DateTime? parsedEndTime);
    }
}
