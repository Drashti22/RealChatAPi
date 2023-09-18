using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using RealChatApi.Models;
using RealChatApi.Repositories;
using System.Security.Claims;

namespace RealChatApi.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _Context;
        public GroupService(IGroupRepository groupRepository, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _groupRepository = groupRepository;
            _httpContextAccessor = httpContextAccessor;
            _Context = context;
        }

        public async Task<GroupResponseDTO> CreateGroup(GroupCreateRequestDTO request)
        {
           if(string.IsNullOrEmpty(request.GroupName))
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

        public async Task<IActionResult>  RetrieveGroupList()
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
    }
}
