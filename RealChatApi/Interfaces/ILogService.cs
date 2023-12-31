﻿using Microsoft.AspNetCore.Mvc;

namespace RealChatApi.Interfaces
{
    public interface ILogService
    {
        Task<IActionResult> getLogs( string startTime, string endTime);
    }
}
