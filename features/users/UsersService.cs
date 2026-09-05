using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using backend.common;
using backend.common.services;
using backend.common.utils;
using backend.features.users.dtos;
using backend.model;
using Microsoft.EntityFrameworkCore;

namespace backend.features.users
{
    public class UsersService(AppDbContext dbContext, CurrentUserService currentUser, StorageService storageService)
    {
        public async Task<Result> GetUserBySearch(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return new Result
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Search value is required."
                };
            }

            var users = await dbContext.Users.AsNoTracking().Where(u =>
             u.Phone.Contains(search) || u.Username.Contains(search) || u.Name.Contains(search)
             ).ToListAsync();

            return new Result
            {
                Status = StatusCodes.Status200OK,
                Data = users
            };
        }

        public async Task<Result> GetUserById(int id)
        {

            var user = await dbContext.Users.AsNoTracking().Select(u => new
            {
                u.Id,
                u.Avatar,
                u.Name,
                u.Username,
            }).SingleOrDefaultAsync(u => u.Id == id);

            return new Result
            {
                Status = StatusCodes.Status200OK,
                Data = user
            };


        }

        public async Task<Result> GetProfile()
        {
            var user = await dbContext.Users.AsNoTracking().Select(u => new
            {
                u.Id,
                u.Username,
                u.Phone,
                u.Avatar,
                u.Name,
                u.Bio
            }).SingleOrDefaultAsync(u => u.Id == currentUser.UserId);

            return new Result
            {
                Status = StatusCodes.Status200OK,
                Data = user
            };
        }

        public async Task<Result> EditProfile(UpdateProfileDto dto)
        {
            string? avatarUrl = null;
            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                avatarUrl = await storageService.UploadFileAsync(dto.Avatar, "avatars");
            }

            await dbContext.Users.Where(u => u.Id == currentUser.UserId).ExecuteUpdateAsync(s =>
            {
                s.SetProperty(u => u.Name, dto.Name)
                .SetProperty(u => u.Bio, dto.Bio);

                if (avatarUrl != null)
                {
                    s.SetProperty(u => u.Avatar, avatarUrl);
                }

            });

            return new Result
            {
                Status = StatusCodes.Status200OK,
                Message = "پروفایل ویرایش شد"
            };
        }



    }
}