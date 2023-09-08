    using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealChatApi.DTOs;
    using RealChatApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

    namespace RealChatApi.Services
    {
        public class UserService : IUserService
        {
            private readonly UserManager<ApplicationUser> _userManager;

            private readonly ApplicationDbContext _authContext;

            public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext authContext)
            {
                _userManager = userManager;
                _authContext = authContext;
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

            public async Task<LoginResponseDto> LoginUserAsync(LoginRequestDto requestDTO)
            {
                if (requestDTO == null)
                    //return IdentityResult.Failed(new IdentityError { Description = "Invalid request." });
                    throw new ArgumentNullException(nameof(requestDTO), "Inavlid Request");

                var user = await _userManager.FindByEmailAsync(requestDTO.email);

                

            if (user == null || !await _userManager.CheckPasswordAsync(user, requestDTO.password))
                throw new InvalidOperationException("Login failed due to incorrect credentials.");

            var token = createJwtToken(user);
            user.Token = token;
            await _userManager.UpdateAsync(user);
            var responseDto = new LoginResponseDto
            {
                Token = token,
                name = user.Name,
                email = user.Email
            };
            return responseDto;
            }

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

    }

}
