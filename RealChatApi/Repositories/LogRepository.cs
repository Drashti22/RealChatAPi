﻿using Microsoft.AspNetCore.Mvc;
using RealChatApi.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RealChatApi.Repositories
{
    


    public class LogRepository : ILogRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public LogRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IQueryable<Log>> getLogs(DateTime? parsedStartTime, DateTime? parsedEndTime)
        {
            var logs =  _applicationDbContext.Logs
                  .Where(log => log.TimeStamp >= parsedStartTime && log.TimeStamp <= parsedEndTime);
                

            return logs;
        }
    }
}
