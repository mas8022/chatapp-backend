using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.features.auth.dtos
{
    public class VerifyOtpResponseDto
    {
        public int Status { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? AccessToken { get; set; }

        public string? SessionId { get; set; }
    }
}