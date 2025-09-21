using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Shopping_Demo.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var userName = user.FindFirstValue(ClaimTypes.Name);
                
                // Nếu là admin, tự động join admin group
                if (user.IsInRole("Admin") || user.IsInRole("Manager") || user.IsInRole("Staff"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
