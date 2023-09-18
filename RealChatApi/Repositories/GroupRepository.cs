using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RealChatApi.DTOs;
using RealChatApi.Models;
using System.Security.Claims;

namespace RealChatApi.Repositories
{
    
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GroupRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task <Group> CreateGroup(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }
        public async Task<List<GetGroupDto>> GetList()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var groupList = await _context.Groups
                .Where(g => g.Members.Any(m => m.Id == userIdClaim))
                .Select(g => new GetGroupDto
                    {
                        Id = g.Id,
                        Name = g.GroupName,
                    })
                .ToListAsync();
            return groupList;
        }
    }
}
