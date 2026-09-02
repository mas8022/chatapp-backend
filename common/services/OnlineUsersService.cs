using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace backend.common.services
{
    public class OnlineUsersService(IDatabase redis)
    {
        private const string OnlineUsersKey = "online_users";

        public async Task AddUserAsync(int userId)
        {
            await redis.SetAddAsync(OnlineUsersKey, userId);
        }

        public async Task RemoveUserAsync(int userId)
        {
            await redis.SetRemoveAsync(OnlineUsersKey, userId);
        }

        public async Task<List<int>> GetOnlineUsersAsync()
        {
            var members = await redis.SetMembersAsync(OnlineUsersKey);
            return members.Select(m => (int)m).ToList();
        }

        public async Task<bool> IsUserOnlineAsync(int userId)
        {
            return await redis.SetContainsAsync(OnlineUsersKey, userId);
        }
    }
}
