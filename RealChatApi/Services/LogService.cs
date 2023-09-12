using Microsoft.AspNetCore.Mvc;
using RealChatApi.Interfaces;
using RealChatApi.Repositories;

namespace RealChatApi.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository logRepository) {
            _logRepository = logRepository;
        }

        public async Task<IActionResult> getLogs(string timeframe, string startTime, string endTime)
        {
            DateTime? parsedStartTime = ParseDateTime(startTime);
            DateTime? parsedEndTime = ParseDateTime(endTime);

            if (parsedStartTime == null)
                parsedStartTime = DateTime.Now.AddMinutes(-5);

            if (parsedEndTime == null)
                parsedEndTime = DateTime.Now;

            switch (timeframe)
            {

                default:
                    break;
            }
            var logs = await _logRepository.getLogs(parsedStartTime, parsedEndTime);

            if (logs == null)
            {
                return new NotFoundObjectResult(new { Message = "Logs not found" });
            }
            return new OkObjectResult(logs);

        }
        private DateTime? ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return null;

            if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
                return parsedDateTime;

            return null;
        }
    }
}
