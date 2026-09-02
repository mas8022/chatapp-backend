using System;
using System.Threading.Tasks;
using backend.common;
using backend.common.services;
using backend.common.utils;
using backend.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace backend.hubs.chat
{
    [Authorize]
    public partial class ChatHub(
        AppDbContext dbContext,
        CurrentUserService user,
        OnlineUsersService onlineUsersService
    ) : Hub
    {
        private int? CurrentUserId => user?.UserId;

        private static string GetPrivateRoomName(int user1, int user2)
        {
            int min = Math.Min(user1, user2);
            int max = Math.Max(user1, user2);
            return $"room_{min}_{max}";
        }

        public override async Task OnConnectedAsync()
        {
            if (CurrentUserId.HasValue)
            {
                await onlineUsersService.AddUserAsync(CurrentUserId.Value);

                await Clients.Group($"presence_user_{CurrentUserId.Value}")
                    .SendAsync("UserPresenceChanged", new { userId = CurrentUserId.Value, isOnline = true });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (CurrentUserId.HasValue)
            {
                await onlineUsersService.RemoveUserAsync(CurrentUserId.Value);

                await Clients.Group($"presence_user_{CurrentUserId.Value}")
                    .SendAsync("UserPresenceChanged", new { userId = CurrentUserId.Value, isOnline = false });
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
