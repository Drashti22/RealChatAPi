namespace RealChatApi.DTOs
{
    public class AddMemberResponseDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public List<string> MembersId { get; set; }
    }
}
