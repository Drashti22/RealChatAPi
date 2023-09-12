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

        public UserController(IUserService userService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager )
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
           
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDTO userobj)
        {
            var result = await _userService.RegisterUserAsync(userobj);

            if(result.Succeeded)
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

        //[HttpPost("GoogleLogin")]
        //public async Task<IdentityResult> GoogleAuthentication([FromBody] GoogleAuthDto requestDto)
        //{
        //    return await _userService.GoogleAuthentication(requestDto);

        //}

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            return await _userService.GetAllUsers();
        }
        //[HttpGet("google")]
        //public IActionResult AuthenticateWithGoogle()
        //{
        //    var properties = new AuthenticationProperties
        //    {
        //        RedirectUri = Url.Action("AuthenticateWithGoogleCallback")
        //    };
        //    return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        //}
        //[HttpGet("google-callback")]
        //public async Task<IActionResult> AuthenticateWithGoogleCallback()
        //{
        //    var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        //    if (!result.Succeeded)
        //    {
        //        var exception = result.Failure;
        //        Console.WriteLine("Exception", exception);
        //        return Unauthorized(new { message = "Google authentication failed." });
        //    }

        //    // Authentication successful, access user information from result.Principal
        //    var user = result.Principal;

        //    // Extract the user's name and email from the user's claims
        //    var name = user.FindFirst(ClaimTypes.Name)?.Value;
        //    var email = user.FindFirst(ClaimTypes.Email)?.Value;

        //    // Generate JWT token
        //    var token = GenerateJwtToken(name, email);

        //    // Return the token along with user information
        //    return Ok(new
        //    {
        //        message = "Google authentication successful",
        //        token = token,
        //        name = name,
        //        email = email
        //    });
        //}

        //[HttpPost("GoogleAuthenticate")]
        //public async Task<IActionResult> GoogleAuthenticate([FromBody] GoogleAuthDto request)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage));
        //    }

        //    var user = await _userService.AuthenticateGoogleUserAsync(request);

        //    if (user != null)
        //    {
        //        var token = createJwtToken(user);
        //        return Ok(new { token = token });
        //    }
        //    else
        //    {
        //        return BadRequest(new { Message = "Google authentication failed." });
        //    }
        //}

        //private string createJwtToken(ApplicationUser user)
        //{
        //    var jwtTokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes("This is my 128 bits very long secret key.......");
        //    var identity = new ClaimsIdentity(new Claim[]
        //    {
        //            new Claim(ClaimTypes.Name, $"{user.Name}"),
        //            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        //    });
        //    var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = identity,
        //        Expires = DateTime.Now.AddDays(3),
        //        SigningCredentials = credentials,
        //    };
        //    var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        //    return jwtTokenHandler.WriteToken(token);
        //}
    }
}
