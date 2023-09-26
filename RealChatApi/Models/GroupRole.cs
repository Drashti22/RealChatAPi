using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealChatApi.Models
{
    public class GroupRole
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("GroupId")]
        public int GroupId { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }

        [Required]
        public string Role { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
