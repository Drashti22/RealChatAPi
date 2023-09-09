using Google.Apis.Auth;
using Lucene.Net.Index;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using RealChatApi.DTOs;
    using RealChatApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace RealChatApi.Services
    {
        public class UserService : IUserService
        {
            private readonly UserManager<ApplicationUser> _userManager;

            private readonly ApplicationDbContext _authContext;

            private readonly IHttpContextAccessor _httpContextAccessor;

            private readonly IConfiguration _configuration;


        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext authContext, IHttpContextAccessor httpContextAccessor)
            {
                _userManager = userManager;
                _authContext = authContext;
                _httpContextAccessor = httpContextAccessor;
        }

            public async Task<IdentityResult> RegisterUserAsync(RegisterRequestDTO requestDTO)
            {
            
                var existingUser = await _userManager.FindByEmailAsync(requestDTO.Email);
                if (existingUser != null)
                    return IdentityResult.Failed(new IdentityError { Description = "Email already exists." });
                

            
                var newUser = new ApplicationUser
                {
                    Name = requestDTO.Name,
                    UserName = requestDTO.Email,
                    Email = requestDTO.Email,
                   
                    
                
                };
                var result = await _userManager.CreateAsync(newUser, requestDTO.Password);
                return result;
            
            }

            public async Task<IActionResult> LoginUserAsync(LoginRequestDto requestDTO)
            {
            if (requestDTO == null)
                //return IdentityResult.Failed(new IdentityError { Description = "Invalid request." });
                return new BadRequestObjectResult(new { Message = "Invalid Request" });

                var user = await _userManager.FindByEmailAsync(requestDTO.email);



            if (user == null || !await _userManager.CheckPasswordAsync(user, requestDTO.password))
                return new NotFoundObjectResult(new { Message = "Login failed due to inavalid credentials" });

            var token = createJwtToken(user);
            user.Token = token;
            await _userManager.UpdateAsync(user);
            var responseDto = new LoginResponseDto
            {
                Token = token,
                name = user.Name,
                email = user.Email
            };
            return new OkObjectResult(
                new
                {
                    Message = "Login Success",
                    Profile = responseDto,
                }

                );
            ;
            }

        //public async Task<IActionResult> AuthenticateGoogleUserAsync(GoogleAuthDto request)
        //{
        //   try
        //    {
        //        var googleUser = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings()
        //        {
        //            Audience = new[] { "http://414420117584-8ggttrr52sgf1cge36h8argahdv4nkaj.apps.googleusercontent.com/" }
        //        }

        //            );
        //   return OkobjectResult();
        //    }
        //    catch
        //    {

        //    }
        //}


        private string createJwtToken(ApplicationUser user)
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("This is my 128 bits very long secret key.......");
                var identity = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, $"{user.Name}"),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                });
                var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = identity,
                    Expires = DateTime.Now.AddDays(3),
                    SigningCredentials = credentials,
                };
                var token = jwtTokenHandler.CreateToken(tokenDescriptor);
                return jwtTokenHandler.WriteToken(token);
            }


        public async Task<ApplicationUser> GetCurrentLoggedInUser()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim != null)
            {
                var currentUser = await _authContext.Users.FirstOrDefaultAsync(u => u.Id == userIdClaim);
                return currentUser;
            }

            return null;
        }

        public async Task<IActionResult> GetAllUsers(){
        var currentUser = await GetCurrentLoggedInUser();
                if(currentUser == null) 
                {
                return new BadRequestObjectResult(new
                {
                    Message = "Unable to retrieve current user."
                });
                }
            var userList = await _authContext.Users
            .Where(u => u.Id != currentUser.Id)
            .Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email,

            })
            .ToListAsync();
            return new OkObjectResult(new { users = userList });
        }
    }


}
