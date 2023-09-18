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
    }
}
