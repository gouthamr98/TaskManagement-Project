using Login.Configurations;
using Login.Data;
using Login.DTOs;
using Login.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Login.Services
{
    public class AuthService:IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbContext db,IOptions<JwtSettings> jwtSettings, IConfiguration config)
        {
            _db = db;
            _jwtHelper=new JwtHelper(jwtSettings);
            _config = config;

        }
        public async Task<Object>LoginAsync(LoginDTO dto)
        {
            var user=await _db.User.Include(u=>u.Role).FirstOrDefaultAsync(u=>u.UserName==dto.UserName && u.IsActive && !u.IsDelete);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid Username or Password.");


            // Generate tokens
            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(60);  // Set refresh token expiry time (can be longer)
            await _db.SaveChangesAsync();

            return new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken  // Send back only the access token ideally
            };
        }


        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"])),
                ValidateLifetime = false // Ignore token expiration
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
}
