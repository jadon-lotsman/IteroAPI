using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Data.Entities;
using Mnemo.Services;

namespace Mnemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        public AccountController(IConfiguration configuration, AccountManagementService accountService)
        {
            _configuration = configuration;
            _accountService = accountService;
        }

        private readonly IConfiguration _configuration;
        private readonly AccountManagementService _accountService;


        [HttpPost("login")]
        public async Task<IActionResult> Login(string username)
        {
            var user = await _accountService.GetByUsernameAsync(username);

            if (user == null)
                return Unauthorized();


            return Ok(GenerateJwt(user));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username)
        {
            var result = await _accountService.CreateAsync(username);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    "USERNAME_TAKEN" => Conflict(result.ErrorCode),
                    "INVALID_PASSWORD" => BadRequest(result.ErrorCode),
                    _ => StatusCode(500, result.ErrorCode)
                };
            }

            return Ok();
        }


        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken (
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
