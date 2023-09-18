using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealChatApi.DTOs;
using RealChatApi.Interfaces;
using RealChatApi.Models;

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

    }
}
