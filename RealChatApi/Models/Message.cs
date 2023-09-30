using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealChatApi.Models
{
    public enum MessageType
    {
        Private,
        Group
    }
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string SenderId { get; set; }


        public MessageType MessageType { get; set; }

        [ForeignKey("SenderId")]
        [JsonIgnore]
        public virtual ApplicationUser Sender { get; set; }

        public string? ReceiverId { get; set; } 

        [ForeignKey("ReceiverId")]
        [JsonIgnore]
        public virtual ApplicationUser? Receiver { get; set; }
                    
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        [JsonIgnore]
        public virtual Group? Group { get; set; }

        public string Content { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
