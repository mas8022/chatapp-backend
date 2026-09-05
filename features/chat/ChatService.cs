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
        public async Task<Result> GetPvMessages(
       int receiverId,
       int page,
       int pageSize,
       CancellationToken cancellationToken)
        {
            var currentUserId = currentUser.UserId;

            // جلوگیری از مقاد  نامعتبر
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = dbContext.Messages
                .AsNoTracking()
                .Where(m =>
                    (m.SenderId == currentUserId &&
                     m.ReceiverId == receiverId) ||

                    (m.SenderId == receiverId &&
                     m.ReceiverId == currentUserId)
                );

            var totalCount = await query.CountAsync(cancellationToken);

            var skip = (page - 1) * pageSize;

            /*
             * چون پیام‌های جدیدتر را اول مرتب می‌کنیم،
             * صفحه اول شامل آخرین پیام‌هاست.
             */
            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    text = m.Content,
                    mediaUrl = m.MediaUrl,
                    time = m.CreatedAt,

                    replyTo = m.ReplyToMessage == null
                        ? null
                        : new
                        {
                            id = m.ReplyToMessage.Id,
                            text = m.ReplyToMessage.Content,
                            mediaUrl = m.ReplyToMessage.MediaUrl
                        }
                })
                .ToListAsync(cancellationToken);

            /*
             * برای نمایش چت، پیام‌های هر صفحه باید از قدیمی به جدید باشند.
             */
            messages.Reverse();

            var hasMore = skip + messages.Count < totalCount;

            return new Result
            {
                Status = StatusCodes.Status200OK,
                Data = new
                {
                    messages,
                    page,
                    pageSize,
                    totalCount,
                    hasMore
                }
            };
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

