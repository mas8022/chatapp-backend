using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.features.auth.dtos
{
    public class RefreshTokenResponseDto
    {
        public int Status { get; set; }

        public string? Message { get; set; } = string.Empty;

        public string? NewAccessToken { get; set; }

        public bool IsAccess { get; set; } = false;
    }
}