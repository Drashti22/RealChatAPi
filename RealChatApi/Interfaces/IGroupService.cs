using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;

namespace RealChatApi.Interfaces
{
    public interface IGroupService
    {
        Task<GroupResponseDTO> CreateGroup(GroupCreateRequestDTO request);

        Task<IActionResult> RetrieveGroupList();

        Task<ApplicationUser> GetCurrentLoggedInUser();

        Task<IActionResult> AddMemberToGroup(int groupId, [FromBody] AddMemberRequestDTO request);

        Task<IActionResult> SendMessage(int groupId, GroupMessageRequestDTO messageRequest);

        Task<IActionResult> GetMessages(int groupId);

        Task<GroupInfoDTO> GetGroupInfo(int groupId);
    }
}
