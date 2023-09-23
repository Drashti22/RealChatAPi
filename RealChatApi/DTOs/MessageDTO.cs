namespace RealChatApi.DTOs
{
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public string SenderId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
