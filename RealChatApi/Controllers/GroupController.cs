﻿using Azure.Core;
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
        public async Task<IActionResult> GetGroups()
        {
            return await _groupService.GetGroups();
        }
        [HttpPut("groups/{groupId}/members")]
        public async Task <IActionResult> UpdateMembers(int groupId, [FromBody] UpdateGroupMembersDTO requset)
        {
            //requset.AddedTime = DateTime.Now;
            return await _groupService.UpdateMembers(groupId, requset);
        }
        [HttpPost("{groupId}/messages")]
        public async Task<IActionResult> SendMessage(int groupId, [FromBody] GroupMessageRequestDTO messageRequest)
        {
            return await _groupService.SendMessage(groupId, messageRequest);
        }
        [HttpGet("{groupId}/messages")]
        public async Task<IActionResult> GetMessages(int groupId)
        {
            return await _groupService.GetGroupMessages(groupId);
        }
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupInfo(int groupId)
        {
            return await _groupService.GetGroupInfo(groupId);
        }
        [HttpDelete("{groupId}")]
        public async Task<IActionResult> RemoveGroup(int groupId)
        {
            return await _groupService.RemoveGroup(groupId);
        }
    }
}
