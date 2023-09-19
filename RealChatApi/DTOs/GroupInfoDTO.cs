namespace RealChatApi.DTOs
{
    public class GroupInfoDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<string> Members { get; set; }
    }
}
