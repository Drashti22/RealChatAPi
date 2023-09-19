using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using System.Text.RegularExpressions;

namespace RealChatApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IGroupService _groupService;

        public GroupController(ApplicationDbContext context, IGroupService groupService)
        {
            _context = context;
            _groupService = groupService;
        }

        [HttpPost]
        public async Task<GroupResponseDTO> CreateGroup([FromBody] GroupCreateRequestDTO request)
        {
            return await _groupService.CreateGroup(request);
        }

        [HttpGet]
        public async Task<IActionResult> RetrieveGroupList()
        {
            return await _groupService.RetrieveGroupList();
        }
        [HttpPost("{groupId}/messages")]
        public async Task<IActionResult> SendMessage(int groupId, [FromBody] GroupMessageRequestDTO messageRequest)
        {
            return await _groupService.SendMessage(groupId, messageRequest);
        }
        [HttpPost("groups/{groupId}/members")]
        public async Task<IActionResult> AddMemberToGroup(int groupId, [FromBody] AddMemberRequestDTO request)
        {
            return await _groupService.AddMemberToGroup(groupId, request);
        }
        [HttpGet("group/{groupId}/Messages")]
        public async Task <IActionResult> GetMessages(int groupId)
        {
            return await _groupService.GetMessages(groupId);
        }

        [HttpGet ("group/{groupId}/groupInfo")]
        public async Task <ActionResult<GroupInfoDTO>> GetGroupInfo(int groupId)
        {
            var groupInfo = await _groupService.GetGroupInfo(groupId);
            if (groupInfo == null)
            {
                return NotFound();
            }

            return Ok(groupInfo);
        }
    }
}
