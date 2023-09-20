namespace RealChatApi.Models
{
    public class GroupMember
    {
        public int Id { get; set; } // Unique identifier for the membership
        public string UserId { get; set; } // ID of the user
        public ApplicationUser User { get; set; } // Reference to the user
        public int GroupId { get; set; } // ID of the group
        public Group Group { get; set; } // Reference to the group                          
    }
}
