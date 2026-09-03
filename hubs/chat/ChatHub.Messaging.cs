using System;
using System.Threading.Tasks;
using backend.common;
using backend.common.utils;
using backend.model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.hubs.chat
{
    public partial class ChatHub : Hub
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

        public async Task SendMessage(string? message, int receiverId, string? mediaUrl = null, int? replyToMessageId = null)
        {
            if (string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(mediaUrl))
                return;

            var currentUserIdInt = Convert.ToInt32(CurrentUserId);

            var entity = new Message
            {
                Content = message?.Trim(),
                MediaUrl = mediaUrl,
                ReceiverId = receiverId,
                SenderId = currentUserIdInt,
                ReplyToMessageId = replyToMessageId,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Messages.Add(entity);
            await dbContext.SaveChangesAsync();

            object? replyToDto = null;
            if (entity.ReplyToMessageId.HasValue)
            {
                replyToDto = await dbContext.Messages
                    .AsNoTracking()
                    .Where(m => m.Id == entity.ReplyToMessageId.Value)
                    .Select(m => new
                    {
                        id = m.Id,
                        text = m.Content,
                        mediaUrl = m.MediaUrl
                    })
                    .FirstOrDefaultAsync();
            }

            var messageDto = new
            {
                id = entity.Id,
                senderId = entity.SenderId,
                text = entity.Content,
                mediaUrl = entity.MediaUrl,
                time = entity.CreatedAt,
                replyTo = replyToDto
            };

            string roomName = GetPrivateRoomName(currentUserIdInt, receiverId);

            await Clients.Group(roomName).SendAsync("ReceiveNewMessage", messageDto);

            await Clients.User(receiverId.ToString())
                .SendAsync("ReceiveLastMessage", messageDto);
        }
      
        public async Task EditPVMessage(int messageId, string newMessage, string receiverId)
        {
            if (string.IsNullOrWhiteSpace(newMessage))
                return;

            var currentUserIdInt = Convert.ToInt32(CurrentUserId);

            var message = await dbContext.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == currentUserIdInt);

            if (message == null)
                return;

            message.Content = newMessage.Trim();
            await dbContext.SaveChangesAsync();

            var roomName = GetPrivateRoomName(currentUserIdInt, Convert.ToInt32(receiverId));

            var updatedMessageDto = new
            {
                id = message.Id,
                text = message.Content,
                senderId = message.SenderId
            };

            await Clients.Group(roomName).SendAsync("UpdatePVMessage", updatedMessageDto);

        }

        
    }
}
