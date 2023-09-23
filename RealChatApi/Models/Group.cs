using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealChatApi.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        public string GroupName { get; set; }

        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    }
}
