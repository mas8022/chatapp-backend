using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace backend.common.services;

public record TokenClaims(int UserId, List<string> Roles, string? SessionId);

public class JwtService(IConfiguration configuration)
{
    // ۱. ساخت Access Token
    public string SignAccessToken(int userId, List<string> roles)
    {
        int minutes = configuration.GetValue<int>("ConnectionStrings:JWT_ACCESS_TOKEN_MINUTE");
        return CreateToken(userId, roles, DateTime.UtcNow.AddMinutes(minutes), tokenType: "access");
    }

    // ۲. ساخت Refresh Token
    public string SignRefreshToken(int userId, List<string> roles, string sessionId)
    {
        int days = configuration.GetValue<int>("ConnectionStrings:JWT_REFRESH_TOKEN_DAY");
        return CreateToken(userId, roles, DateTime.UtcNow.AddDays(days), tokenType: "refresh", sessionId: sessionId);
    }

    // ۳. اعتبارسنجی Access Token و استخراج کلیم‌ها
    public TokenClaims VerifyAccessToken(string token)
    {
        var principal = VerifyToken(token);
        if (principal.FindFirst("token_type")?.Value != "access")
            throw new SecurityTokenException("توکن ارسالی Access Token نیست.");

        return ExtractClaims(principal);
    }

    // ۴. اعتبارسنجی Refresh Token و استخراج کلیم‌ها
    public TokenClaims VerifyRefreshToken(string token)
    {
        var principal = VerifyToken(token);
        if (principal.FindFirst("token_type")?.Value != "refresh")
            throw new SecurityTokenException("توکن ارسالی Refresh Token نیست.");

        return ExtractClaims(principal);
    }

    // استخراج راحت کلیم‌ها از ClaimsPrincipal
    private static TokenClaims ExtractClaims(ClaimsPrincipal principal)
    {
        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst("sub")?.Value
                     ?? throw new SecurityTokenException("شناسه کاربر در توکن نامعتبر است.");

        var userId = int.Parse(userIdStr);

        var roles = principal.FindAll(c => c.Type == ClaimTypes.Role || c.Type == "role")
                             .Select(c => c.Value)
                             .ToList();

        var sessionId = principal.FindFirst("session_id")?.Value;

        return new TokenClaims(userId, roles, sessionId);
    }

    // تولید توکن
    private string CreateToken(int userId, List<string> roles, DateTime expires, string tokenType, string? sessionId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("token_type", tokenType)
        };

        if (!string.IsNullOrWhiteSpace(sessionId))
            claims.Add(new Claim("session_id", sessionId));

        foreach (var role in roles.Distinct())
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            configuration.GetConnectionString("JWT_SECRET_KEY")!));

        var token = new JwtSecurityToken(
            issuer: configuration.GetConnectionString("JWT_ISSUER"),
            audience: configuration.GetConnectionString("JWT_AUDIENCE"),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // بررسی صحت توکن
    private ClaimsPrincipal VerifyToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new SecurityTokenException("توکن ارسال نشده است.");

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration.GetConnectionString("JWT_SECRET_KEY")!)),
            ValidateIssuer = true,
            ValidIssuer = configuration.GetConnectionString("JWT_ISSUER"),
            ValidateAudience = true,
            ValidAudience = configuration.GetConnectionString("JWT_AUDIENCE"),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
    }
}
