using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.common;
using backend.model;
using Microsoft.EntityFrameworkCore;

namespace backend.features.users
{
    public class UsersService(AppDbContext dbContext)
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

            var user = await dbContext.Users.AsNoTracking().Select(u =>new
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








    }
}