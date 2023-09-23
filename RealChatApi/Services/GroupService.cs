using Azure;
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

        public GroupService(UserManager<ApplicationUser> userManager, IGroupRepository groupRepository, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _groupRepository = groupRepository;
            _httpContextAccessor = httpContextAccessor;
            _Context = context;
            _hubContext = hubContext;
            _userManager = userManager;
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
            var response = new GroupResponseDTO
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                //Members = group.Members.Select(u => u.Id).ToList()
            };
            return response;

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
        public async Task<IActionResult> AddMember(int groupId, AddMemberReqDTO requset)
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
            foreach (var memberId in requset.GroupMembers)
            {
                var existingMember = group.GroupMembers.FirstOrDefault(gm => gm.UserId == memberId);

                if (existingMember == null)
                {
                    var newMember = new GroupMember
                    {
                        UserId = memberId,
                        GroupId = groupId
                    };
                    group.GroupMembers.Add(newMember);
                }
            }
            await _Context.SaveChangesAsync();
            var response = new AddMemberResDTO
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                GroupMembers = group.GroupMembers.Select(gm => gm.User?.Name).ToList()
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
            var messages = await _groupRepository.GetGroupMessagesAsync(groupId);
           
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
    }
}
