using EduSense.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, RefreshToken> _refreshTokens = [];

        private readonly ILogger _logger;




        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> CreateToken([FromBody] AuthModel authRequest)
        {
            
            if (authRequest.Username == "initializeMaster")
            {
                if (!authRequest.AllowInit())
                {
                    return BadRequest();
                }

                try
                {
                    return Ok(await _coreRepositoryService.InitMasterUser());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            var user = await Authenticate(authRequest);
            if (user != null)
            {
                return Ok(GenerateToken(user));
            }

            return Unauthorized();
        }


        //Post: api/auth/login
        //Tar emot användarnamn och lösenord, autentiserar användaren och returnerar en JWT-token om autentiseringen lyckas.

        //[HttpPost("login")]

        public async Task <IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _signInManager.PasswordSignInAsync(
                model.Username, model.Password,isPersistent: false, lockoutOnFailure: false);

            if (!user.Succeeded)
            {
                return Unauthorized("fel användarnamn eller lösenord");
            }

            return Ok("Inloggning lyckades");

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Skapa ett nytt IdentityUser-objekt
            var user = new IdentityUser 
            { UserName = model.Username, Email = model.Email };

            // CreateAsync hashar lösenordet och sparar användaren i databasen
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(errors);
            }

            return Ok("Registrering lyckades");
        }

        // POST /api/auth/logout
        // Loggar ut användaren genom att ta bort Identity-cookien
        // [Authorize] krävs, man måste vara inloggad för att logga ut

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Utloggning lyckades");
        }
       
    }
}
