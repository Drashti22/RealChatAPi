using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task <Group> CreateGroup(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }
        
        public async Task<Group> FindGroup(int groupid)
        {
           var group=  await _context.Groups.FindAsync(groupid);
            return group;
        }
        public async Task<bool> groupIdExists(int groupId)
        {
            return _context.Groups.Any(r => r.Id == groupId);
        }


        public async Task<List<Group>> GetGroups()
        {

            var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (currentUser == null)
            {
                return null; // Handle the case where the user is not found
            }
            var userGroups = await _context.GroupMembers
                .Include(gm => gm.Group.GroupMembers)
                .Where(gm => gm.UserId == currentUser.Id)
                .Select(gm => gm.Group)
                .ToListAsync();
            return userGroups;
        }
        public async Task<bool> IsUserMemberOfGroup(string userId, int groupId)
        {
            var isMember = await _context.GroupMembers
           .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);

            return isMember;
        }
        public async Task<Group> GetGroupWithMembersAsync(int groupId)
    {
        // Retrieve the group with its associated GroupMembers
        var group = await _context.Groups
            .Include(g => g.GroupMembers)
            .ThenInclude(gm => gm.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        return group;
        }
        public async Task<Message> CreateMessageAsync(Message message)
        {
            var AddedMessage = _context.Messages.Add(message).Entity;
            await _context.SaveChangesAsync();
            return AddedMessage;

        }
        public async Task<Group> GetGroupAsync(int groupId)
        {

            return await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }
        public async Task<IEnumerable<Message>> GetGroupMessagesAsync(int groupId)
        {
            return await _context.Messages
                .Where(message => message.GroupId == groupId)
                .OrderBy(message => message.Timestamp)
                .ToListAsync();
        }
        public async Task<IEnumerable<Message>> GetGroupCurrentMessagesAsync(int groupId, DateTime fromDate)
        {
            return await _context.Messages
                .Where(message => message.GroupId == groupId && message.Timestamp >= fromDate)
                .OrderBy(message => message.Timestamp)
                .ToListAsync();
        }
        public async Task<List<string>> GetGroupMemberIdsAsync(int groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Select(gm => gm.UserId)
                .ToListAsync();
        }
        public async Task<Group> RemoveGroup(Group group)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return group;

        }
    }
}
