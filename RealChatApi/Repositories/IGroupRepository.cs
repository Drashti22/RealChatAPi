using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface IGroupRepository
    {
        Task<Group> CreateGroup(Group group);

        Task<List<GetGroupDto>> GetList();

        Task<Group> FindGroup(int groupid);

        Task<ApplicationUser> AddUser(ApplicationUser user);
        Task<Group> UpdateGroup(Group group);
        Task<Message> SendMessage (Message message);

        Task<bool> groupIdExists (int groupId);

        Task<Group> GetGroupInfo(int groupId);

    }
}
