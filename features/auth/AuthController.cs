using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.common;
using backend.features.auth.dtos;
using backend.features.auth.dtos.sendOtp;
using Microsoft.AspNetCore.Mvc;

namespace backend.features.auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthService authService, IHostEnvironment env) : ControllerBase
    {
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp(SendOtpDto dto)
        {
            return Ok(await authService.SendOtp(dto.Phone));
        }
        
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var result = await authService.VerifyOtp(dto);

            if (!string.IsNullOrWhiteSpace(result.AccessToken) && !string.IsNullOrWhiteSpace(result.SessionId))
            {
                Response.Cookies.Append("access_token", result.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = env.IsDevelopment() ? false : true,
                    SameSite = env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });

                Response.Cookies.Append("session_id", result.SessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = env.IsDevelopment() ? false : true,
                    SameSite = env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

            }

            return Ok(new Result
            {
                Status = result.Status,
                Message = result.Message
            });
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await authService.RefreshToken(Request.Cookies["access_token"], Request.Cookies["session_id"]);

            if (!string.IsNullOrEmpty(result.NewAccessToken))
            {


                Response.Cookies.Append("access_token", result.NewAccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = env.IsDevelopment() ? false : true,
                    SameSite = env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });
            }

            return Ok(new Result
            {
                Status = result.Status,
                Data= result.IsAccess
            });

        }

        [HttpDelete("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await authService.Logout(Request.Cookies["session_id"]);

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("session_id");

            return Ok(result);
        }

    }
}