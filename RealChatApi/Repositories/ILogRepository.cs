using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface ILogRepository
    {
        Task<IQueryable<Log>> getLogs(DateTime? parsedStartTime, DateTime? parsedEndTime);
    }
}
