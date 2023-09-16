using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealChatApi.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string SenderId { get; set; }


        
        [ForeignKey("SenderId")]
        [JsonIgnore]
        public virtual ApplicationUser Sender { get; set; }

        public string ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        [JsonIgnore]
        public virtual ApplicationUser Receiver { get; set; }


        public string Content { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
