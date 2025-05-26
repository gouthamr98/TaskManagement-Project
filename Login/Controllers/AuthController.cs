using Login.Configurations;
using Login.Data;
using Login.DTOs;
using Login.Helpers;
using Login.Models;
using Login.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Login.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _db;
        private readonly JwtHelper _jwtHelper;

        public AuthController(IAuthService authService, ApplicationDbContext db, IOptions<JwtSettings> jwtSettings)
        {
            _authService = authService;
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _jwtHelper = new JwtHelper(jwtSettings);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(token);
        }



        //[Authorize]
        //[HttpGet("me")]
        //public IActionResult GetCurrentUser()
        //{
        //    // Safely extract claims
        //    var username = User.FindFirst(ClaimTypes.Name)?.Value;
        //    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var role = User.FindFirst(ClaimTypes.Role)?.Value;

        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdStr))
        //        return Unauthorized("Token does not contain valid user data.");

        //    if (!int.TryParse(userIdStr, out int userId))
        //        return Unauthorized("Invalid user ID in token.");

        //    // Optional: fetch additional user data from DB if needed
        //    var user = _db.User
        //        .Where(u => u.UserId == userId)
        //        .Select(u => new
        //        {
        //            u.UserId,
        //            u.UserName,
        //            Role = role,
        //            u.RefreshTokenExpiryTime // optional
        //        })
        //        .FirstOrDefault();

        //    if (user == null)
        //        return NotFound("User not found.");

        //    return Ok(user);
        //}


       // [Authorize]
        [HttpGet("me")]
        public IActionResult GetSomething()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var tokenStr = authHeader.Substring("Bearer ".Length).Trim();

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenStr); 

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
              
                if (userId == null)
                    return Unauthorized("User ID not found in token");
    
                var parsedUserId = int.Parse(userId);
                var user = _db.User.FirstOrDefault(u => u.UserId == parsedUserId);
                if (user == null)
                    return NotFound("User not found");

                return Ok(new
                {
                    user.UserId,
                    user.UserName,
                    user.Email,
                    user.RefreshTokenExpiryTime
                });
            }

            return Unauthorized();
        }


        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] TokenResponseDTO tokens)
        {
            var principal = _authService.GetPrincipalFromExpiredToken(tokens.AccessToken);
            var useridClaim = principal?.Claims.FirstOrDefault(c=>c.Type==ClaimTypes.NameIdentifier)?.Value;
            int userid = 0;
            if(!string.IsNullOrEmpty(useridClaim))
            {
                userid = int.Parse(useridClaim);
            }
            var user = _db.User.SingleOrDefault(u => u.UserId == userid);
            //if (user == null || user.RefreshToken != tokens.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            //{
            //    return BadRequest("Invalid refresh token");
            //}
            //if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            //{
            //    return BadRequest("Invalid refresh token");
            //}
            var newAccessToken = _jwtHelper.GenerateAccessToken(user);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken(user);

           // user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _db.SaveChanges();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken  // Send back only the access token ideally
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterDTO dto)
        {
            var exists = await _db.User.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return BadRequest("Email already exists.");

            var user = new Users
            {
                UserName = dto.UserName,
                Email = dto.Email,
                RoleID = dto.RoleId,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                RefreshToken = "", 
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            };

            _db.User.Add(user);
            await _db.SaveChangesAsync();

            return Ok(user);
        }

    }
}
