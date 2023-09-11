using Microsoft.AspNetCore.SignalR;

namespace RealChatApi
{
    public sealed class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} has connected.");
            await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
        }
    }
}
