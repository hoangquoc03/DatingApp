using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace DatingApp.Hubs
{
    public class ChatHub : Hub
    {
        // Thread-safe: dùng ConcurrentDictionary thay vì HashSet
        // Key = userId, Value = số lượng connection đang active (hỗ trợ nhiều tab/thiết bị)
        private static readonly ConcurrentDictionary<string, int> OnlineUserConnections = new();

        /// <summary>
        /// Kiểm tra user có đang online không
        /// </summary>
        public static bool IsUserOnline(string userId)
            => OnlineUserConnections.ContainsKey(userId);

        /// <summary>
        /// Lấy danh sách tất cả user đang online
        /// </summary>
        public static IEnumerable<string> GetOnlineUsers()
            => OnlineUserConnections.Keys;

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                // Tăng connection count (hỗ trợ user mở nhiều tab)
                var isNewlyOnline = OnlineUserConnections.AddOrUpdate(
                    userId,
                    addValue: 1,
                    updateValueFactory: (_, count) => count + 1
                ) == 1;

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    userId
                );

                // Chỉ broadcast "online" khi user vừa lên mạng (connection đầu tiên)
                if (isNewlyOnline)
                {
                    await Clients.All.SendAsync(
                        "UserOnline",
                        userId
                    );
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                var isNowOffline = false;

                OnlineUserConnections.AddOrUpdate(
                    userId,
                    addValue: 0,
                    updateValueFactory: (key, count) =>
                    {
                        var newCount = count - 1;
                        if (newCount <= 0)
                        {
                            isNowOffline = true;
                            // Trả về 0, sẽ remove bên dưới
                            return 0;
                        }
                        return newCount;
                    }
                );

                // Xóa khỏi dictionary nếu không còn connection nào
                if (isNowOffline)
                {
                    OnlineUserConnections.TryRemove(userId, out _);

                    await Clients.All.SendAsync(
                        "UserOffline",
                        userId
                    );
                }
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