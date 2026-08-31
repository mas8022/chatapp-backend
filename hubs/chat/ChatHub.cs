using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.hubs.chat
{
    [Authorize]
    public class ChatHub(AppDbContext dbContext) : Hub
    {
        public async Task SendMessage(string message, int receiverId)
        {


            if (string.IsNullOrWhiteSpace(message)) return;

            var senderIdClaim = Context.UserIdentifier
                                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(senderIdClaim) || !int.TryParse(senderIdClaim, out int senderId))
            {
                throw new HubException("User is not authenticated.");
            }

            var entity = new Message
            {
                Content = message.Trim(),
                ReceiverId = receiverId,
                SenderId = senderId,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Messages.Add(entity);
            await dbContext.SaveChangesAsync();

            var conversationMessages = await dbContext.Messages
                .AsNoTracking()
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.SenderId,
                    m.CreatedAt
                })
                .ToListAsync();

            var messagesForSender = conversationMessages.Select(m => new
            {
                id = m.Id,
                text = m.Content,
                isMe = m.SenderId == senderId,
                time = m.CreatedAt.ToString("HH:mm")
            }).ToList();

            var messagesForReceiver = conversationMessages.Select(m => new
            {
                id = m.Id,
                text = m.Content,
                isMe = m.SenderId == receiverId,
                time = m.CreatedAt.ToString("HH:mm")
            }).ToList();

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessages", messagesForReceiver);
            await Clients.Caller.SendAsync("ReceiveMessages", messagesForSender);
        }
    }
}
