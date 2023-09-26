namespace RealChatApi.DTOs
{
    public class UpdateGroupMembersDTO
    {
        public List<string> MembersToAdd { get; set; }
        public List<string> MembersToRemove { get; set; }
    }
}
