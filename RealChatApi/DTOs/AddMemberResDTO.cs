using RealChatApi.Models;

namespace RealChatApi.DTOs
{
    public class AddMemberResDTO
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public List<string> GroupMembers { get; set; }
    }
}
