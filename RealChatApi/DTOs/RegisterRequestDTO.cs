﻿using System.ComponentModel.DataAnnotations;

namespace RealChatApi.DTOs
{
    public class RegisterRequestDTO
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
    }
}
