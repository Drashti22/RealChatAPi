using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Models;

namespace RealChatApi.Services
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterRequestDTO requestDTO);
        Task<IActionResult> LoginUserAsync(LoginRequestDto requestDTO);

        Task<IActionResult> GetAllUsers();

        Task<IdentityUser> AuthenticateGoogleUserAsync(GoogleAuthDto request);

        Task<object> GoogleAuthenticate(GoogleAuthDto request);
    }
}