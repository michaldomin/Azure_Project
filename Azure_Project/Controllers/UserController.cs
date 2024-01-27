using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure_Project.Configuration;
using Azure_Project.Data;
using Azure_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Azure_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtConfig _jwtConfig;

        public UserController(ApplicationDbContext context, JwtConfig jwtConfig)
        {
            _context = context;
            _jwtConfig = jwtConfig;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == user.Username);
            if (userExists)
            {
                return BadRequest(new { message = "Username already exists." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(User loginUser)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginUser.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
                {
                    return Unauthorized(new { message = "Username or password is incorrect" });
                }

                var token = GenerateJwtToken(user);
                var tokenDto = new TokenDto { Token = token };

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append(_jwtConfig.TokenCookieName, token, cookieOptions);

                return Ok(tokenDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request." });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}