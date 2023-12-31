﻿  using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using RealChatApi.Models;
using RealChatApi.Repositories;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Group = RealChatApi.Models.Group;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RealChatApi.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _Context;
        private readonly static connection<string> _connections = new connection<string>();
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, string> userGroupMapping = new Dictionary<string, string>();

        public GroupService(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, IGroupRepository groupRepository, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _groupRepository = groupRepository;
            _httpContextAccessor = httpContextAccessor;
            _Context = context;
            _hubContext = hubContext;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<GroupResponseDTO> CreateGroup(GroupCreateRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.GroupName))
            {
                throw new ArgumentException("Group name is required.");
            }
            var currentUser = await GetCurrentLoggedInUser();

            if (currentUser == null)
            {

                throw new Exception("Unable to retrieve currentuser");

            }
            var group = new Group
            {
                GroupName = request.GroupName,

            };
            var groupMember = new GroupMember
            {
                UserId = currentUser.Id,
                User = currentUser,
                Group = group,
            };
            group.GroupMembers.Add(groupMember);
            await _groupRepository.CreateGroup(group);
            await AssignGroupRole(groupMember, "Admin", group.Id);

            var response = new GroupResponseDTO
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                //Members = group.Members.Select(u => u.Id).ToList()
            };
           

            return response;

        }
        private async Task AssignGroupRole(GroupMember groupMember, string roleName, int groupId)
        {

            var existingRole = await _Context.GroupRoles.FirstOrDefaultAsync(gr => gr.GroupId == groupId && gr.UserId == groupMember.UserId);

            if (existingRole == null)
            {

                var groupRole = new GroupRole
                {
                    GroupId = groupId,
                    UserId = groupMember.UserId,
                    Role = roleName,
                };

                _Context.GroupRoles.Add(groupRole);
                await _Context.SaveChangesAsync();
            }
        }
        public async Task<ApplicationUser> GetCurrentLoggedInUser()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim != null)
            {
                var currentUser = await _Context.Users.FirstOrDefaultAsync(u => u.Id == userIdClaim);
                return currentUser;
            }

            return null;
        }
        public async Task<IActionResult> GetGroups()
        {
            var currentUser = await GetCurrentLoggedInUser();
            if (currentUser == null)
            {
                return new NotFoundObjectResult("User not found");
            }

            var userGroups = await _groupRepository.GetGroups();

            return new OkObjectResult(userGroups);
        }
        private bool IsUserAdminInGroup(int groupId, string userId)
        {
            Console.WriteLine($"Checking admin status for groupId: {groupId}, userId: {userId}");
            return _Context.GroupRoles.Any(gr => gr.GroupId == groupId && gr.UserId == userId && gr.Role == "Admin");
        }
        public async Task<IActionResult> UpdateMembers(int groupId, UpdateGroupMembersDTO requset)
        {
            var currentUser = await GetCurrentLoggedInUser();
            if (currentUser == null)
            {
                return new NotFoundObjectResult("User not found");
            }
            var isMemberOfGroup = await _groupRepository.IsUserMemberOfGroup(currentUser.Id, groupId);
            if (!isMemberOfGroup)
            {
                return new UnauthorizedObjectResult("You are not a member of this group.");
            }
            var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
            if (group == null)
            {
                return new NotFoundObjectResult("Group not found.");
            }
            bool isAdmin = await _Context.GroupRoles.AnyAsync(gr => gr.GroupId == groupId && gr.UserId == currentUser.Id && gr.Role == "Admin");
            Console.WriteLine($"isAdmin: {isAdmin}");
            Console.WriteLine($"IsUserAdminInGroup result for groupId: {groupId}, userId: {currentUser.Id}: {isAdmin}");


            DateTime timestampBeforeAddingMembers = DateTime.Now;
            Console.WriteLine($"Timestamp before adding members: {timestampBeforeAddingMembers}");
            Console.WriteLine($"IncludePreviousChat value: {requset.IncludePreviousChat}");
            foreach (var memberId in requset.MembersToAdd)
            {
                var existingMember = group.GroupMembers.FirstOrDefault(gm => gm.UserId == memberId);

                if (existingMember == null)
                {
                    var timestampNow = DateTime.Now;
                    bool include = requset.IncludePreviousChat;
                    var newMember = new GroupMember
                    {
                        UserId = memberId,
                        GroupId = groupId,
                        JoinTime = timestampNow,  // Set the timestamp
                        IncludePreviousChat = include
                    };
                    Console.WriteLine($"JoinTime for new member: {timestampNow}");
                    Console.WriteLine($"IncludePreviousChat value: {requset.IncludePreviousChat}");
                    group.GroupMembers.Add(newMember);
                    foreach (var connectionId in _connections.GetConnections(memberId))
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveGroupUpdate", groupId);
                    }
                 
                    Console.WriteLine($"JoinTime for new member: {timestampNow}");
                    Console.WriteLine($"IncludePreviousChat value: {requset.IncludePreviousChat}");

                   
                    await _Context.SaveChangesAsync();
                    Console.WriteLine($"JoinTime for new member: {timestampNow}");
                    Console.WriteLine($"IncludePreviousChat value: {requset.IncludePreviousChat}");

                }
                else
                {                            
                    _Context.Entry(existingMember).State = EntityState.Detached;
                }
            }


            foreach (var memberId in requset.MembersToRemove)
            {
               
                var userToRemove = await _userManager.FindByIdAsync(memberId);

                if (userToRemove != null)
                {
                    bool isUserAdminInGroup = IsUserAdminInGroup(groupId, userToRemove.Id);
                    Console.WriteLine($"isUserAdminInGroup for user {userToRemove.Id}: {isUserAdminInGroup}");
                    // Check if the current user is an admin and if the user to remove is also an admin
                    if (isAdmin || (!isAdmin && !isUserAdminInGroup))
                    {
                        // Remove the user from the group
                        var memberToRemove = group.GroupMembers.FirstOrDefault(gm => gm.UserId == memberId);
                        if (memberToRemove != null)
                        {
                            group.GroupMembers.Remove(memberToRemove);
                            
                        }
                    }
                    else
                    {
                        return new UnauthorizedObjectResult("You cannot remove an admin from the group.");
                    }
                    await _Context.SaveChangesAsync();
                    foreach (var connectionId in _connections.GetConnections(memberId))
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveGroupUpdate", groupId);
                    }
                }
            }
            var result = await _groupRepository.GetGroupWithMembersAsync(groupId);
            var response = new AddMemberResDTO                       
            {
                GroupId = result.Id,
                GroupName = result.GroupName,
                GroupMembers = result.GroupMembers
                                .Where(gm => gm.User != null)
                                .Select(gm => gm.User.Name)
                                .ToList()

            };

            return new OkObjectResult(response);
        }
       
        public async Task<IActionResult> SendMessage(int groupId, GroupMessageRequestDTO messageRequest)
        {

            var currentUser = await GetCurrentLoggedInUser();

            var isMemberOfGroup = await _groupRepository.IsUserMemberOfGroup(currentUser.Id, groupId);
            if (!isMemberOfGroup)
            {
                return new UnauthorizedObjectResult("You are not a member of this group.");
            }
            if (string.IsNullOrEmpty(messageRequest.content))
            {
                throw new ArgumentException("Group name is required.");
            }
            var group = await _groupRepository.GetGroupAsync(groupId);
            if (group == null)
            {
                return new NotFoundObjectResult("Group not found.");
            }
            var message = new Message
            {
                Content = messageRequest.content,
                Sender = currentUser,
                Group = group,
                Timestamp = DateTime.Now
            };

            await _groupRepository.CreateMessageAsync(message);
            var memberIds = await _groupRepository.GetGroupMemberIdsAsync(groupId);
                                         
            foreach (var memberId in memberIds)
            {
                foreach (var connectionId in _connections.GetConnections(memberId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("GroupMessage", message);
                }
            }
            var response = new
            {
                messageId = message.Id,
                senderId = currentUser.Id,
                groupId = group.Id,
                content = message.Content,
                timestamp = message.Timestamp
            };

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> GetGroupMessages(int groupId)
        {
            var currentUser = await GetCurrentLoggedInUser();

            var isMemberOfGroup = await _groupRepository.IsUserMemberOfGroup(currentUser.Id, groupId);
            if (!isMemberOfGroup)
            {
                return new UnauthorizedObjectResult("You are not a member of this group.");
            }
            var group = await _groupRepository.GetGroupAsync(groupId);

            if (group == null)
            {
                return new NotFoundObjectResult("Group not found.");
            }

            var memberPreferences = await _groupRepository.GetMemberPreferencesAsync(currentUser.Id, groupId);

            IEnumerable<Message> messages;

            if (memberPreferences.IncludePreviousChatPreference)
            {
                // Retrieve all messages for the group
                messages = await _groupRepository.GetGroupMessagesAsync(groupId);
            }
            else
            {
                // Retrieve messages after the specified timestamp for the group
                messages = await _groupRepository.GetMessagesAfterTimestampAsync(groupId, memberPreferences.TimestampOfMemberAdded);
            }

            return new OkObjectResult(messages);
        }
        public async Task<IActionResult> GetGroupInfo(int groupId)
        {
            var currentUser = await GetCurrentLoggedInUser();

            var isMemberOfGroup = await _groupRepository.IsUserMemberOfGroup(currentUser.Id, groupId);
            if (!isMemberOfGroup)
            {
                return new UnauthorizedObjectResult("You are not a member of this group.");
            }
            var group = await _groupRepository.GetGroupAsync(groupId);

            if (group == null)
            {
                return new NotFoundObjectResult("Group not found.");
            }
            var memberIds = await _groupRepository.GetGroupMemberIdsAsync(groupId);


            // Retrieve user names based on member IDs
            var memberNames = new List<string>();
            foreach (var memberId in memberIds)
            {
                var user = await _userManager.FindByIdAsync(memberId);
                if (user != null)
                {
                    memberNames.Add(user.Name);
                }
            }
            var response = new
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                Members = memberNames
            };
            return new OkObjectResult(response);
        }

        public async Task<IActionResult> RemoveGroup (int groupId)
        {
            var currentUser = await GetCurrentLoggedInUser();
            var isMemberOfGroup = await _groupRepository.IsUserMemberOfGroup(currentUser.Id, groupId);
            if (!isMemberOfGroup)
            {
                return new UnauthorizedObjectResult("You are not a member of this group.");
            }
            var group = await _groupRepository.GetGroupAsync(groupId);

            if (group == null)
            {
                return new NotFoundObjectResult("Group not found.");
            }
            bool isAdmin = await _Context.GroupRoles.AnyAsync(gr => gr.GroupId == groupId && gr.UserId == currentUser.Id && gr.Role == "Admin");
            if (isAdmin)
            {
               
                var memberIds = await _groupRepository.GetGroupMemberIdsAsync(groupId);
                var removedGroup = await _groupRepository.RemoveGroup(group);

                foreach (var memberId in memberIds)
                {
                    foreach (var connectionId in _connections.GetConnections(memberId))
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveGroupUpdate", groupId);
                    }
                }
                if (removedGroup != null)
                {
                    return new OkObjectResult(new {message = "Group Removed Successfully!!"});
                }
                else
                {
                    return new NotFoundObjectResult(new {message = "Group could not be removed." });
                }
            }
            else
            {
                return new UnauthorizedObjectResult("You are not authorized to remove this group.");
            }
        }
    }
}
