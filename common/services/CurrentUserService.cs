using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.common.utils
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        private readonly ClaimsPrincipal? User = httpContextAccessor.HttpContext?.User;

        public int? UserId
        {
            get
            {
                var userIdClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub") ?? User?.FindFirstValue("id");

                if (int.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
                return userId;
            }
        }

            public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    }
}





