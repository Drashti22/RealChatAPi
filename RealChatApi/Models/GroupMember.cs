using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RealChatApi.Models
{
    public class GroupMember
    {
        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }
        public int GroupId { get; set; }

        [JsonIgnore]
        public Group Group { get; set; }               
    }
}
