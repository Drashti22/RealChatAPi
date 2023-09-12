using RealChatApi.Models;

namespace RealChatApi.DTOs
{
    public class RegisterResponseDTO
    {
        public bool Success { get; set; }
        public Message User { get; set; }
        public RegistrationErrorType ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum RegistrationErrorType
    {
        None,
        Validation,
        Conflict
    }
}
