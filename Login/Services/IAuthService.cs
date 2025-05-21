using Login.DTOs;
using System.Security.Claims;

namespace Login.Services
{
    public interface IAuthService
    {
        Task<Object> LoginAsync(LoginDTO dto);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
