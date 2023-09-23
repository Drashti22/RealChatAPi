using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;

namespace RealChatApi.Interfaces
{
    public interface IGroupService
    {
        Task<GroupResponseDTO> CreateGroup(GroupCreateRequestDTO request);

        Task<ApplicationUser> GetCurrentLoggedInUser();

        Task<IActionResult> GetGroups();

        Task<IActionResult> AddMember(int groupId, AddMemberReqDTO requset);

        Task<IActionResult> SendMessage(int groupId, GroupMessageRequestDTO messageRequest);

        Task<IActionResult> GetGroupMessages(int groupId);

        Task<IActionResult> GetGroupInfo(int groupId);

    }
}
