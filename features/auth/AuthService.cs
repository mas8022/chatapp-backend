using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.common;
using backend.common.services;
using backend.common.utils;
using backend.features.auth.dtos;
using backend.model;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace backend.features.auth;

public class AuthService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    IDatabase redis,
    AppDbContext dbContext,
    JwtService jwtService,
    GenerateUniqueUsername generateUniqueUsername
)
{
    public async Task<Result> SendOtp(string phone)
    {
        var user = configuration.GetConnectionString("FARAZSMS_USER");
        var pass = configuration.GetConnectionString("FARAZSMS_PASS");
        var fromNum = configuration.GetConnectionString("FARAZSMS_FROM_NUM");
        var patternCode = configuration.GetConnectionString("FARAZSMS_PATTERN_CODE");

        var code = RandomNumberGenerator
            .GetInt32(10000, 100000)
            .ToString();

        var payload = new
        {
            op = "pattern",
            user = user,
            pass = pass,
            fromNum = fromNum,
            toNum = phone,
            patternCode = patternCode,
            inputData = new[]
            {
                new Dictionary<string, string>
                {
                    ["verification-code"] = code
                }
            }
        };

        var client = httpClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "https://ippanel.com/api/select",
            payload
        );

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerException(
                $"Failed to reach SMS service. Response: {responseBody}"
            );
        }

        if (
            responseBody.Contains("error", StringComparison.OrdinalIgnoreCase) ||
            responseBody.Contains("961") ||
            responseBody.Contains("960")
        )
        {
            throw new InternalServerException(
                $"SMS Provider Error: {responseBody}"
            );
        }

        await redis.StringSetAsync(
            $"otp:{phone}",
            code,
            TimeSpan.FromMinutes(2)
        );

        return new Result
        {
            Status = StatusCodes.Status200OK,
            Message = "Verification code sent successfully."
        };
    }

    public async Task<VerifyOtpResponseDto> VerifyOtp(VerifyOtpDto dto)
    {
        string otpKey = $"otp:{dto.Phone}";

        string? savedCode = await redis.StringGetAsync(otpKey);

        if (string.IsNullOrEmpty(savedCode))
        {
            return new VerifyOtpResponseDto
            {
                Status = StatusCodes.Status403Forbidden,
                Message = "Verification code has expired or was not found."
            };
        }

        if (savedCode != dto.Code)
        {
            return new VerifyOtpResponseDto
            {
                Status = StatusCodes.Status401Unauthorized,
                Message = "Invalid verification code."
            };
        }

        await redis.KeyDeleteAsync(otpKey);

        var user = await dbContext.Users
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(user => user.Phone == dto.Phone);

        if (user == null)
        {
            bool isFirstUser = !await dbContext.Users.AnyAsync();

            var userRole = await dbContext.Roles
                .FirstOrDefaultAsync(role => role.Name == "USER");

            if (userRole == null)
            {
                throw new InternalServerException(
                    "Default role 'USER' was not found in the Roles table."
                );
            }

            user = new User
            {
                Phone = dto.Phone,
                Username = await generateUniqueUsername.Execute(dbContext)
            };

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();

            await dbContext.UserRoles.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            });

            if (isFirstUser)
            {
                var managerRole = await dbContext.Roles
                    .FirstOrDefaultAsync(role => role.Name == "MANAGER");

                if (managerRole == null)
                {
                    throw new InternalServerException(
                        "Role 'MANAGER' was not found in the Roles table."
                    );
                }

                await dbContext.UserRoles.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = managerRole.Id
                });
            }

            await dbContext.SaveChangesAsync();

            user = await dbContext.Users
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .FirstAsync(u => u.Id == user.Id);
        }

        List<string> roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .Distinct()
            .ToList();

        string sessionId = Guid.NewGuid().ToString();

        string accessToken = jwtService.SignAccessToken(
            user.Id,
            roles
        );

        string refreshToken = jwtService.SignRefreshToken(
            user.Id,
            roles,
            sessionId
        );

        await redis.StringSetAsync(
            $"refresh_token:{sessionId}",
            refreshToken,
            TimeSpan.FromDays(7)
        );

        return new VerifyOtpResponseDto
        {
            Status = StatusCodes.Status200OK,
            Message = "Login successful.",
            AccessToken = accessToken,
            SessionId = sessionId
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshToken(string? accessToken, string? sessionId)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            try
            {
                jwtService.VerifyAccessToken(accessToken);
                return new RefreshTokenResponseDto
                {
                    Status = 200,
                    IsAccess = true
                };
            }
            catch
            {
            }
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return new RefreshTokenResponseDto
            {
                Status = 403,
                Message = "Please log in to your account.",
                IsAccess = false
            };
        }

        var rawRefresh = await redis.StringGetAsync($"refresh_token:{sessionId}");

        if (rawRefresh.IsNullOrEmpty)
        {
            return new RefreshTokenResponseDto
            {
                Status = 403,
                Message = "Please log in again.",
                IsAccess = false
            };
        }

        try
        {
            var claims = jwtService.VerifyRefreshToken(rawRefresh.ToString());

            var newAccessToken = jwtService.SignAccessToken(claims.UserId, claims.Roles);

            return new RefreshTokenResponseDto
            {
                Status = 200,
                NewAccessToken = newAccessToken,
                IsAccess = true
            };
        }
        catch
        {
            return new RefreshTokenResponseDto
            {
                Status = 403,
                Message = "Invalid token. Please log in again.",
                IsAccess = false
            };
        }
    }

    public async Task<Result> Logout(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return new Result
            {
                Status = 400,
                Message = "Session ID not found."
            };
        }

        await redis.KeyDeleteAsync($"refresh_token:{sessionId}");

        return new Result
        {
            Status = 200,
            Message = "Logged out successfully."
        };
    }
}
