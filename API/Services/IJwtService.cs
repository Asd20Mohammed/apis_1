using WebApi.Models;
using System.Security.Claims;

namespace WebApi.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
    bool IsTokenExpired(string token);
    DateTime GetTokenExpiration(string token);
} 