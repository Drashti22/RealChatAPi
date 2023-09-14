
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealChatApi.DTOs;
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


        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext authContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _userManager = userManager;
            _authContext = authContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
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
                Token = ""


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
                Id = user.Id,
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


        public string createJwtToken(ApplicationUser user)
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

        public async Task<IActionResult> GetAllUsers()
        {
            var currentUser = await GetCurrentLoggedInUser();
            if (currentUser == null)
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
        public async Task<String> GoogleAuthenticate(GoogleAuthDto request)
        {
            try
            {
                var user = await AuthenticateGoogleUserAsync(request);
                if (user == null)
                {
                    // Log or handle the case where authentication failed.
                    return "Authentication failed";
                }

                var token = CreateToken(user);
                return token;       
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes.
                Console.WriteLine($"Exception in GoogleAuthenticate: {ex}");
                throw; // Rethrow the exception for proper handling.
            }
        }
        public async Task<IdentityUser> AuthenticateGoogleUserAsync(GoogleAuthDto request)
        {
            try
            {
                if (request == null)
                {
                    // Handle the case where the request is null.
                    return null; // or throw an exception or return an error response
                }

                Payload payload = null;

                if (_configuration != null)
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.idToken, new ValidationSettings
                    {
                        Audience = new[] { _configuration["Google:ClientId"] }
                    });
                }

                if (payload == null)
                {
                    // Handle the case where the payload is null.
                    return null; // or throw an exception or return an error response
                }

                return await GetOrCreateExternalLoginUser(GoogleAuthDto.PROVIDER, payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
            }
            catch (InvalidJwtException ex)
            {
                // Handle the exception, log it, and return an error response.
                Console.WriteLine($"Exception in AuthenticateGoogleUserAsync: {ex}");
                return null; // or throw an exception or return an error response
            }
        }



        private async Task<ApplicationUser> GetOrCreateExternalLoginUser(string provider, string key, string email, string firstName, string lastName)
        {
            var user = await _userManager.FindByLoginAsync(provider, key);


            if (user != null)
                return user;

            user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // If the email is not found, try to create the user with the provided firstName as the username
                user = new ApplicationUser
                {
                    Name = firstName,
                    Email = email,
                    UserName = firstName,
                    Id = key,
                    Token= ""
                };

            }
            var userName = await _userManager.FindByNameAsync(firstName);
            if (userName != null)
            {
                // If the email exists and the username (firstName) is also taken, generate a unique username
                string newUserName = firstName;
                int count = 1;
                while (userName != null)
                {
                    newUserName = $"{firstName}{count:D2}"; // Appending a unique number to the username
                    userName = await _userManager.FindByNameAsync(newUserName);
                    count++;
                }
                user.UserName = newUserName;
                await _userManager.UpdateAsync(user);
            }
            await _userManager.CreateAsync(user);
            var info = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
            var result = await _userManager.AddLoginAsync(user, info);

            if (result.Succeeded)
                return user;

            return null;
        }

        private string CreateToken(IdentityUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                 //new Claim("UserId", user.UserID.ToString())
                //new Claim(ClaimTypes.Role,"User")
            };

            //var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("This is my 128 bits very long secret key.......").Value))
            var key = Encoding.ASCII.GetBytes("This is my 128 bits very long secret key.......");
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;

        }
    }


}
