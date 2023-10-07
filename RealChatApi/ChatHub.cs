using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealChatApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Security;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

namespace RealChatApi
{
    public class ChatHub : Hub
    {
        private readonly connection<string> _connections = new connection<string>();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub( IHttpContextAccessor httpContextAccessor)
        {
            Console.WriteLine("ChatHub instance created.");

            _httpContextAccessor = httpContextAccessor;
        }

        //Typing notification
        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = GetUserId();
            Console.WriteLine($"User {userId} connected with connection ID {connectionId}");

            _connections.Add(userId, connectionId);
            Console.WriteLine($"User {userId} connected with connection ID {connectionId}");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            var connectionId = Context.ConnectionId;

            _connections.Remove(userId, connectionId);
            Console.WriteLine($"User {userId} disconnected with connection ID {connectionId}");

            return base.OnDisconnectedAsync(exception);
        }
        private string GetUserId()
        {
            var query = Context.GetHttpContext().Request.Query;
            var token = query["access_token"];

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Missing access_token in query string");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
               throw new InvalidOperationException("User ID claim not found in JWT token");
            }
            var userId = userIdClaim.Value;

            return userId;
        }
        [HubMethodName("GroupMembersUpdated")]
        public async Task NotifyGroupMembersUpdated(int groupId, string[] groupMembers)
        {
            Console.WriteLine($"NotifyGroupMembersUpdated called for Group ID: {groupId}");
            try
            {

                await Clients.Group(groupId.ToString()).SendAsync("GroupMembersUpdated", groupId, groupMembers);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error in NotifyGroupMembersUpdated: {ex.Message}");
                throw; // Rethrow the exception if needed
            }
        }
        

        public async Task JoinGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }
    }
}
