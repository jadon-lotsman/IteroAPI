using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Mnemo.Data.Entities;
using Mnemo.Services;
using Mnemo.Services.Queries;
using Mnemo.Contracts.Dtos.Account.Requests;
using Mnemo.Common;

namespace Mnemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountQueries _accountQueries;
        private readonly AccountManagementService _accountService;

        public AccountController(IConfiguration configuration, AccountQueries accountQueries, AccountManagementService accountService)
        {
            _configuration = configuration;
            _accountQueries = accountQueries;
            _accountService = accountService;
        }

        private readonly IConfiguration _configuration;



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _accountQueries.GetByUsernameAsync(request.Username);

            if (user == null)
                return Unauthorized();


            return Ok(GenerateJwt(user));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _accountService.CreateAsync(request.Username);

            if (!result.IsSuccess)
            {
                return result.ErrorCode switch
                {
                    ErrorCode.UsernameTaken => Conflict(result.ErrorMessage),
                    ErrorCode.InvalidPassword => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return Created();
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
