using EduSense.BLL.Services;
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
        private readonly IPasswordResetService _passwordResetService;

       

        private readonly IWebHostEnvironment _environment;
        private readonly EduSense.BLL.Services.IEmailSender _emailSender;

        public AuthController(UserManager<ApplicationUser> userManager, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, IPasswordResetService passwordResetService, IWebHostEnvironment environment, EduSense.BLL.Services.IEmailSender emailSender)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _passwordResetService = passwordResetService;
            _environment = environment;
            _emailSender = emailSender;
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

            // Kontrollera att önskat användarnamn inte redan används av någon annan -
            // annars vinner den som råkar spara sist tyst över den andra (FindByNameAsync
            // matchar på NormalizedUserName, så jämförelsen är skiftlägesokänslig).
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                return BadRequest(new[] { "Användarnamnet är redan taget." });
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            // SetUserNameAsync istället för att sätta user.UserName direkt - den uppdaterar
            // även NormalizedUserName, annars matchar inte FindByNameAsync/inloggning.
            var userNameResult = await _userManager.SetUserNameAsync(user, dto.UserName);
            if (!userNameResult.Succeeded)
            {
                return BadRequest(userNameResult.Errors.Select(e => e.Description));
            }

            // DisplayName sattes aldrig när kontot skapades (inbjudan känner bara till
            // e-post, inte det slutgiltiga användarnamnet) - sätts nu till samma som
            // det valda användarnamnet, annars visas den bara som "?"/e-postadressen
            // i UI:t (t.ex. floating-nav-avataren, "Skapad av"-kolumner).
            user.DisplayName = dto.UserName;
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username is null || userId is null)
            {
                return Unauthorized();
            }

            var displayName = User.FindFirst("display_name")?.Value ?? username;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return Ok(new UserInfoDto { Id = userId, Username = username, DisplayName = displayName, Roles = roles });
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
                // Dynamisk istället för alltid true - annars skickar webbläsaren
                // aldrig med cookien över vanlig http (t.ex. LAN-dev-läget i
                // start-lan-dev.ps1), medan https-profilerna/produktion ändå
                // alltid får Secure=true eftersom de requestarna faktiskt är https.
                Secure = Request.IsHttps,
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
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenExpiry
            });

            return new CreateTokenResponseDto(accessTokenExpiry);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgetPassword(ForgotPasswordDto dto)
        {
            var token = await _passwordResetService.ForgotPasswordAsync(dto.Email);

            // Säkerhet: svara alltid OK för att inte läcka om e-post finns.
            if (token is not null)
            {
                var baseUrl = _configuration["ClientApp:BaseUrl"] ?? "https://localhost:7289";
                var url = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={Uri.EscapeDataString(token)}";

                var body = $"""
            <p>Du har begärt återställning av lösenord.</p>
            <p><a href="{url}">Klicka här för att byta lösenord</a></p>
            <p>Om du inte begärde detta kan du ignorera mailet.</p>
            """;

                await _emailSender.SendAsync(dto.Email, "Återställ lösenord", body);
            }

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _passwordResetService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);

            if (!result)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
