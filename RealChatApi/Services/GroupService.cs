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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RealChatApi.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _Context;
        private readonly static connection<string> _connections = new connection<string>();
        private readonly IHubContext<ChatHub> _hubContext;
        public GroupService(IGroupRepository groupRepository, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _groupRepository = groupRepository;
            _httpContextAccessor = httpContextAccessor;
            _Context = context;
            _hubContext = hubContext;
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
                Members = new List<ApplicationUser> { currentUser }
            };
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

        public async Task<IActionResult> RetrieveGroupList()
        {
            var currentUser = await GetCurrentLoggedInUser();
            if (currentUser == null)
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Unable to retrieve current user."
                });
            }
            var groupList = await _groupRepository.GetList();
            return new OkObjectResult(new { groups = groupList });
        }

        public async Task<IActionResult> SendMessage(int groupId, GroupMessageRequestDTO messageRequest)
        {
            var group = await _groupRepository.FindGroup(groupId);
            var currentUser = await GetCurrentLoggedInUser();
            if (group == null)
            {
                return new NotFoundObjectResult(new
                {
                    Message = "Group not found"
                });
            }
            bool IsUserMemberOfGroup(ApplicationUser user, Group group)
            {
                if (group == null || group.Members == null)
                {
                    return false; // The group or its members collection is null, so the user cannot be a member.
                }
                return group.Members.Contains(user);
            }
            if (!IsUserMemberOfGroup(currentUser, group))
            {

                return new UnauthorizedObjectResult(new
                {
                    Message = "You are not a member of this group and cannot send messages to it."
                });
            }
            if (string.IsNullOrEmpty(messageRequest.content))
            {
                throw new ArgumentException("Group name is required.");
            }
            MessageType messageType;
            if (group.Id == groupId)
            {
                messageType = MessageType.Group;
            }
            else
            {
                messageType = MessageType.Private;
            }
            

            var message = new Message
            {
                Content = messageRequest.content,
                Sender = currentUser,
                ReceiverId = null,
                Receiver = (ApplicationUser)group.Members,
                Group = group,
                Timestamp = DateTime.UtcNow
            };
            
            await _groupRepository.SendMessage(message);
            
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

        public async Task<IActionResult> AddMemberToGroup(int groupId, [FromBody] AddMemberRequestDTO request)
        {
            //try
            //{
            //    var group = await _groupRepository.FindGroup(groupId);
            //    if (group == null)
            //    {
            //        return new NotFoundObjectResult(new
            //        {
            //            Message = "Group not found"
            //        });
            //    }
            //    var currentUser = await GetCurrentLoggedInUser();
            //    bool IsUserMemberOfGroup(ApplicationUser user, Group group)
            //    {
            //        if (group == null || group.Members == null)
            //        {
            //            return false; // The group or its members collection is null, so the user cannot be a member.
            //        }
            //        return group.Members.Contains(user);
            //    }
            //    if (!IsUserMemberOfGroup(currentUser, group))
            //    {
            //        return new UnauthorizedObjectResult(new
            //        {
            //            Message = "You are not a member of this group and cannot send messages to it."
            //        });
            //    }
            //    foreach (var member in request.Members)
            //    {
            //        // Check if the member to add exists in the database
            //        var userToAdd = await _Context.Users.FirstOrDefaultAsync(u => u.Id == member);
            //        if (userToAdd != null)
            //        {
            //            if (group == null)
            //            {
            //                group = new Group(); // Create a new group if it's null
            //                                     // Initialize other properties of 'group' as needed
            //            }
            //            if (group.Members == null)
            //            {
            //                group.Members = new List<ApplicationUser>(); // Initialize 'Members' collection if it's null
            //            }

            //            // Add the user to the group if not already a member
            //            if (!group.Members.Contains(userToAdd))
            //            {
            //                group.Members.Add(userToAdd);
            //            }

            //        }
            //    }
            //    await _groupRepository.UpdateGroup(group);
            //    var response = new AddMemberResponseDTO
            //    {
            //        GroupId = group.Id,
            //        GroupName = group.GroupName,
            //        MembersId = group.Members.Select(u => u.Id).ToList()
            //    };
            //    return new OkObjectResult(response);
            //}
            //catch (Exception ex)
            //{
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            
        }

        public async Task<IActionResult> GetMessages(int groupId)
        {
            var authenticatedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var sender = await _messageRepository.FindUserByEmailAsync(currentUserID);
            var currentUser = await GetCurrentLoggedInUser();
            var group = await _groupRepository.FindGroup(groupId);
            if (authenticatedUserId == null)
            {
                return new UnauthorizedResult();
            }
            bool IsUserMemberOfGroup(ApplicationUser user, Group group)
            {
                //int groupId = group.Id;
                //var users = _Context.
                if (group == null || group.Members.Count == 0)
                {
                    return false; // The group or its members collection is null, so the user cannot be a member.
                }
                return group.Members.Contains(user);
            }
            bool isUserMember = IsUserMemberOfGroup(currentUser, group);
            if (!isUserMember)
            {
                return new UnauthorizedObjectResult(new
                {
                    Message = "You are not a member of this group and cannot send messages to it."
                });
            }
            var currentUserID = new ApplicationUser { Id = authenticatedUserId };
            var groupIdExists = await _groupRepository.groupIdExists(groupId);
            if (!groupIdExists)
            {
                return new NotFoundObjectResult(new { Message = "Group does not exist" });
            }
            
            var groupMessages = await RetrieveGroupMessages(groupId);

            if (groupMessages == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(new { messages = groupMessages });
        }

        public async Task<IEnumerable<MessageDTO>> RetrieveGroupMessages(int groupId)
        {
            // Ensure that the group exists
            var group = await _groupRepository.FindGroup(groupId);
            var currentUser = await GetCurrentLoggedInUser();
            bool IsUserMemberOfGroup(ApplicationUser user, Group group)
            {
                if (group == null || group.Members == null)
                {
                    return false; // The group or its members collection is null, so the user cannot be a member.
                }
                return group.Members.Contains(user);
            }
            if (!IsUserMemberOfGroup(currentUser, group))
            {
                return null;
            }
            if (group == null)
            {
                return null; 
            }
            
            // Fetch group messages from the database
            var groupMessages = await _Context.Messages
                .Where(message => message.GroupId == groupId)
                .OrderBy(message => message.Timestamp)
                .Select(message => new MessageDTO
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    Content = message.Content,
                    Timestamp = message.Timestamp
                })
                .ToListAsync();

            

            return groupMessages;
        }

        public async Task<GroupInfoDTO> GetGroupInfo(int groupId)
        {

            var group = await _groupRepository.FindGroup(groupId);
            var currentUser = await GetCurrentLoggedInUser();
            bool IsUserMemberOfGroup(ApplicationUser user, Group group)
            {
                if (group == null || group.Members == null)
                {
                    return false; // The group or its members collection is null, so the user cannot be a member.
                }
                return group.Members.Contains(user);
            }
            if (!IsUserMemberOfGroup(currentUser, group))
            {
                return null;
            }
            if (group == null)
            {
                return null;
            }
            var groupInfo = await _groupRepository.GetGroupInfo(groupId);
            var groupInfoList = new GroupInfoDTO
            {
                GroupId = group.Id,
                GroupName = group.GroupName,
                Members = group.Members.Select(u => u.Id).ToList()
            };
            return groupInfoList;
        }
    }
}
