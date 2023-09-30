using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;
using System.Text.RegularExpressions;

namespace RealChatApi.Interfaces
{
    public interface IGroupService
    {
        Task<GroupResponseDTO> CreateGroup(GroupCreateRequestDTO request);

        Task<ApplicationUser> GetCurrentLoggedInUser();

        Task<IActionResult> GetGroups();

        Task<IActionResult> UpdateMembers(int groupId, UpdateGroupMembersDTO requset);

        Task<IActionResult> SendMessage(int groupId, GroupMessageRequestDTO messageRequest);

        Task<IActionResult> GetGroupMessages(int groupId, bool includePreviousChat );

        Task<IActionResult> GetGroupInfo(int groupId);

        Task<IActionResult> RemoveGroup( int groupId);

    }
}
