using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ControllerBase
    {
        private static readonly string[] InvitableRoles = { "Admin", "Analyst" };

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost("invite")]
        public async Task<IActionResult> Invite([FromBody] InviteUserRequestDto dto)
        {
            if (!InvitableRoles.Contains(dto.Role))
            {
                return BadRequest($"Ogiltig roll. Tillåtna roller: {string.Join(", ", InvitableRoles)}");
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = false,
                    IsActive = false
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(createResult.Errors.Select(e => e.Description));
                }
            }

            // Säkerställ att användaren har rätt roll (och bara den efterfrågade rollen)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            // Token förnyas varje gång - gammal länk blir automatiskt ogiltig
            // eftersom SecurityStamp uppdateras internt av UserManager vid behov.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var clientBaseUrl = _configuration["ClientApp:BaseUrl"];
            var inviteLink = $"{clientBaseUrl}/skapa-losenord?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendAsync(
                dto.Email,
                "Du är inbjuden till EduSense",
                $"<p>Du har blivit inbjuden som <b>{dto.Role}</b>. Klicka <a href=\"{inviteLink}\">här</a> för att skapa ditt lösenord. Länken gäller i 24 timmar.</p>");

            return Ok("Inbjudan skickad.");
        }
    }
}