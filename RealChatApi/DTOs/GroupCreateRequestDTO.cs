using System.ComponentModel.DataAnnotations;

namespace RealChatApi.DTOs
{
    public class GroupCreateRequestDTO
    {
        [Required(ErrorMessage = "Group name is required.")]
        public string GroupName { get; set; }

    }
}
