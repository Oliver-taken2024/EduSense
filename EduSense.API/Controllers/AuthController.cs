using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const int AccessTokenMinutes = 15;
        private const int RefreshTokenDays = 7;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        [HttpPost("set-initial-password")]
        [AllowAnonymous]
        public async Task<IActionResult> SetInitialPassword([FromBody] SetInitialPasswordRequestDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user is null)
            {
                return BadRequest("Ogiltig länk.");
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            user.EmailConfirmed = true;
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Ok("Lösenord skapat. Du kan nu logga in.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized("Fel användarnamn eller lösenord");
            }

            var response = await CreateTokenResponseAsync(user);
            return Ok(response);
        }

        //[HttpPost("register")]
        //[AllowAnonymous]
        //public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        //{
        //    var user = new ApplicationUser { UserName = dto.Username, Email = dto.Email };

        //    var result = await _userManager.CreateAsync(user, dto.Password);

        //    if (!result.Succeeded)
        //    {
        //        var errors = result.Errors.Select(e => e.Description);
        //        return BadRequest(errors);
        //    }

        //    return Ok("Registrering lyckades");
        //}

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized("Ingen refresh token skickades");
            }

            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (stored is null || stored.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized("Ogiltig eller utgången refresh token");
            }

            await _refreshTokenRepository.RemoveAsync(stored); // rotera - engångsbruk

            var user = await _userManager.FindByIdAsync(stored.UserId);
            if (user is null)
            {
                return Unauthorized();
            }

            var response = await CreateTokenResponseAsync(user);
            return Ok(response);
        }

        // POST /api/auth/logout
        // Loggar ut användaren genom att ta bort refresh-token-cookien
        // [Authorize] krävs, man måste vara inloggad för att logga ut

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (stored is not null)
                {
                    await _refreshTokenRepository.RemoveAsync(stored);
                }
            }
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok("Utloggning lyckades");
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var username = User.Identity?.Name;
            if (username is null)
            {
                return Unauthorized();
            }

            var displayName = User.FindFirst("display_name")?.Value ?? username;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return Ok(new UserInfoDto { Username = username, DisplayName = displayName, Roles = roles });
        }

        private async Task<CreateTokenResponseDto> CreateTokenResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName!),
                new("display_name", user.DisplayName ?? user.UserName!)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: accessTokenExpiry,
                signingCredentials: credentials);
             
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = accessTokenExpiry
            });

            var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(RefreshTokenDays);

            await _refreshTokenRepository.AddAsync(new RefreshTokenModel
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiry
            });

            Response.Cookies.Append("refreshToken", refreshTokenValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenExpiry
            });

            return new CreateTokenResponseDto(accessTokenExpiry);
        }
    }
}
