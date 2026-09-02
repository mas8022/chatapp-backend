using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace backend.hubs.chat
{
    public partial class ChatHub
    {
        public async Task<bool> SubscribeToUserPresence(int targetUserId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"presence_user_{targetUserId}");
            return await onlineUsersService.IsUserOnlineAsync(targetUserId);
        }

        public async Task UnsubscribeFromUserPresence(int targetUserId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"presence_user_{targetUserId}");
        }
    }
}