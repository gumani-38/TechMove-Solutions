using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechMove_API.Data;

namespace TechMove_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly TechMoveDbContext _context;
        public AuthController(IConfiguration config, TechMoveDbContext context)
        {
            _config = config;
            _context = context;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {

            var user = _context.Users.SingleOrDefault(u => u.Email == model.Username);
            if (user == null)
            {
                // return invalid credentials response
                return BadRequest("Invalid username or password");
            }
            // bcrypt verification
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    // return invalid credentials response
                    return BadRequest("Invalid username or password");
            }
            if (user != null)
            {
                var token = GenerateJwtToken(model.Username);
                return Ok(new { token });
            }

            return Unauthorized();
        }

        private string GenerateJwtToken(string username)
        {
            var keyString = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyString))
                throw new InvalidOperationException("Jwt:Key is missing from configuration.");

            var keyBytes = Encoding.UTF8.GetBytes(keyString);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
                                                           SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    public class LoginModel 
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
}
