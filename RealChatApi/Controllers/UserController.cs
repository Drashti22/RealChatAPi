using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealChatApi.DTOs;
using RealChatApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealChatApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace RealChatApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(IUserService userService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;

        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDTO userobj)
        {
            var result = await _userService.RegisterUserAsync(userobj);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    Message = "User Registered !!",
                    User = new
                    {
                        //userId = userobj.Id,
                        name = userobj.Name,
                        email = userobj.Email
                    }
                }

                    );
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { Message = "Registration failed", Errors = errors });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDto userobj)
        {
            return await _userService.LoginUserAsync(userobj);
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            return await _userService.GetAllUsers();
        }
      

        [HttpPost("GoogleAuthenticate")]
        public async Task<object> GoogleAuthenticate([FromBody] GoogleAuthDto request)
        {
            return await _userService.GoogleAuthenticate(request);
        }
        
    }
}
