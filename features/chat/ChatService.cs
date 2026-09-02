using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.common;
using backend.common.services;
using backend.common.utils;
using backend.model;
using Microsoft.EntityFrameworkCore;

namespace backend.features.chat
{
    public class ChatService(AppDbContext dbContext, CurrentUserService currentUser)
    {
        public async Task<Result> GetPvMessages(int receiverId, CancellationToken cancellationToken)
        {
            var senderId = currentUser.UserId;

            var isExistReceiver = await dbContext.Users.AnyAsync(u => u.Id == receiverId, cancellationToken);

            if (!isExistReceiver) throw new NotFoundException("این کاربر پیدا نشد");


            var messages = await dbContext.Messages
                .AsNoTracking()
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    text = m.Content,
                    mediaUrl = m.MediaUrl,
                    time = m.CreatedAt,

                    replyTo = m.ReplyToMessage == null ? null : new
                    {
                        id = m.ReplyToMessage.Id,
                        text = m.ReplyToMessage.Content,
                        mediaUrl = m.ReplyToMessage.MediaUrl
                    }
                }).ToListAsync(cancellationToken);


            return new Result { Status = StatusCodes.Status200OK, Data = messages };
        }

        public async Task<Result> GetContacts()
        {
            var userId = currentUser.UserId;

            // ۱. استخراج شناسه آخرین پیام هر چت (به صورت کاملاً بهینه در SQL)
            var latestMessageIds = await dbContext.Messages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.Max(m => m.Id))
                .ToListAsync();

            // ۲. دریافت اطلاعات آخرین پیام و کاربر متناظر
            var contacts = await dbContext.Messages
                .AsNoTracking()
                .Where(m => latestMessageIds.Contains(m.Id))
                .Select(m => new
                {
                    ContactId = m.SenderId == userId ? m.ReceiverId : m.SenderId,
                    LastMessage = m.Content,
                    LastMessageTime = m.CreatedAt
                })
                .Join(
                    dbContext.Users.AsNoTracking(),
                    msg => msg.ContactId,
                    user => user.Id,
                    (msg, user) => new
                    {
                        Id = user.Id,
                        Name = user.Name ?? user.Username,
                        Avatar = user.Avatar,
                        LastMessage = msg.LastMessage,
                        LastMessageTime = msg.LastMessageTime
                    }
                )
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

            return new Result
            {
                Status = StatusCodes.Status200OK,
                Data = contacts
            };
        }


    };
}

