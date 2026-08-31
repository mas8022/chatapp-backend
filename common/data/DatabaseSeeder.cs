using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.model;
using Microsoft.EntityFrameworkCore;

namespace backend.common.data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // نقش‌های پیش‌فرض
            var defaultRoles = new[] { "USER", "MANAGER", "ADMIN" };

            foreach (var roleName in defaultRoles)
            {
                bool exists = await dbContext.Roles.AnyAsync(r => r.Name == roleName);
                if (!exists)
                {
                    await dbContext.Roles.AddAsync(new Role { Name = roleName });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }

}








