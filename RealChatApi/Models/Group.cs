using System.ComponentModel.DataAnnotations;

namespace RealChatApi.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        public string GroupName { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; } 

        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    }
}
