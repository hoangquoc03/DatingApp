using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DatingApp.Hubs
{
    public class ChatHub : Hub
    {
        public static HashSet<string> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                OnlineUsers.Add(userId);

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    userId
                );

                await Clients.All.SendAsync(
                    "UserOnline",
                    userId
                );
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                OnlineUsers.Remove(userId);

                await Clients.All.SendAsync(
                    "UserOffline",
                    userId
                );
            }

            await base.OnDisconnectedAsync(ex);
        }
        public async Task Typing(string receiverId)
        {
            var senderId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Clients.Group(receiverId)
                .SendAsync("Typing", senderId);
        }
    }
}