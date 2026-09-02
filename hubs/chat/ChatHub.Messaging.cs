using System;
using System.Threading.Tasks;
using backend.common;
using backend.common.utils;
using backend.model;
using Microsoft.AspNetCore.SignalR;

namespace backend.hubs.chat
{
    public partial class ChatHub
    {

        

        public async Task JoinPrivateChat(int targetUserId)
        {
            string roomName = GetPrivateRoomName(Convert.ToInt32(CurrentUserId), targetUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task LeavePrivateChat(int targetUserId)
        {
     

            string roomName = GetPrivateRoomName(Convert.ToInt32(CurrentUserId), targetUserId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        public async Task SendMessage(string? message, int receiverId, string? mediaUrl = null)
        {


            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(mediaUrl))
                return;

            var entity = new Message
            {
                Content = message?.Trim(),
                MediaUrl = mediaUrl,
                ReceiverId = receiverId,
                SenderId = Convert.ToInt32(CurrentUserId),
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Messages.Add(entity);
            await dbContext.SaveChangesAsync();

            var messageDto = new
            {
                id = entity.Id,
                senderId = entity.SenderId,
                text = entity.Content,
                mediaUrl = entity.MediaUrl,
                time = entity.CreatedAt.ToString("o")
            };

            string roomName = GetPrivateRoomName(Convert.ToInt32(CurrentUserId), receiverId);

            await Clients.Group(roomName).SendAsync("ReceiveNewMessage", messageDto);

            await Clients.User(receiverId.ToString())
                .SendAsync("ReceiveNewMessage", messageDto);
        }
    }
}
