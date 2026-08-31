using backend.model;
using Microsoft.EntityFrameworkCore;

namespace backend.common.utils
{
    public class GenerateUniqueUsername
    {
        public async Task<string> Execute(AppDbContext dbContext)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            while (true)
            {
                string randomPart = new string(
                    Enumerable.Range(0, 8)
                        .Select(_ => chars[Random.Shared.Next(chars.Length)])
                        .ToArray()
                );

                string username = $"user_{randomPart}";

                bool exists = await dbContext.Users
                    .AnyAsync(user => user.Username == username);

                if (!exists)
                {
                    return username;
                }
            }
        }
    }
}
