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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserListItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Visar bara admin/analyst-konton i den här vyn - respondenter
                // hanteras separat via token och ska inte dyka upp här.
                if (!roles.Any(r => InvitableRoles.Contains(r)))
                {
                    continue;
                }

                result.Add(new UserListItemDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    DisplayName = user.DisplayName,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                });
            }
            return Ok(result.OrderBy(u => u.Email).ToList());
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

            // S�kerst�ll att anv�ndaren har r�tt roll (och bara den efterfr�gade rollen)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            await SendInviteEmailAsync(user, dto.Role);

            return Ok("Inbjudan skickad.");
        }

        [HttpPost("{id}/resend-invite")]
        public async Task<IActionResult> ResendInvite(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound("Användaren hittades inte.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault(r => InvitableRoles.Contains(r));
            if (role is null)
            {
                return BadRequest("Användaren saknar en giltig roll att bjuda in med.");
            }

            await SendInviteEmailAsync(user, role);

            return Ok("Inbjudan skickad på nytt.");
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound("Användaren hittades inte.");
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            return Ok("Användaren är nu inaktiverad.");
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound("Användaren hittades inte.");
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return Ok("Användaren är nu aktiverad.");
        }

        private async Task SendInviteEmailAsync(ApplicationUser user, string role)
        {
            // Token förnyas varje gång - gammal länk blir automatiskt ogiltig.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var clientBaseUrl = _configuration["ClientApp:BaseUrl"];
            var inviteLink = $"{clientBaseUrl}/skapa-losenord?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendAsync(
                user.Email!,
                "Du är inbjuden till EduSense",
                $"<p>Du har blivit inbjuden som <b>{role}</b>. Klicka <a href=\"{inviteLink}\">här</a> för att skapa ditt lösenord. Länken gäller i 24 timmar.</p>");
        }
    }
}