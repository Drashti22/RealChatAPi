﻿using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;

namespace RealChatApi.Repositories
{
    public interface IGroupRepository
    {
        Task<Group> CreateGroup(Group group);
        Task<bool> groupIdExists (int groupId);

        Task<Group> FindGroup(int groupid);

        Task<Group> GetGroupAsync(int groupId);

        Task<List<Group>> GetGroups();

        Task<bool> IsUserMemberOfGroup(string userId, int groupId);

        Task<Group> GetGroupWithMembersAsync(int groupId);

        Task<Message> CreateMessageAsync(Message message);

        Task<IEnumerable<Message>> GetGroupMessagesAsync(int groupId);
        Task<IEnumerable<Message>> GetMessagesAfterTimestampAsync(int groupId, DateTime timestamp);

        // Task SendPreviousChatHistoryAsync(int groupId, IEnumerable<Message> previousChat, string newMemberId, DateTime timestampBeforeAddingMembers, bool includePreviousChat = false);
        Task<GroupMemberPreferences> GetMemberPreferencesAsync(string userId, int groupId);
        Task<List<string>> GetGroupMemberIdsAsync(int groupId);

        Task<Group> RemoveGroup(Group group);

    }
}
