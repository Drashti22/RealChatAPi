using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface IGroupRepository
    {
        Task<Group> CreateGroup(Group group);

        Task<List<GetGroupDto>> GetList();
    }
}
