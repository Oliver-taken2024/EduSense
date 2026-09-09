using System.Net.Http.Json;
using System.Security.Claims;
using EduSense.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace EduSense.UI.Services
{
    // Håller reda på om användaren är inloggad genom att fråga API:et (GET api/auth/me),
    // eftersom JWT:n ligger i en httpOnly-cookie som C#/JS aldrig kan läsa direkt.
    public class CookieAuthenticationStateProvider(HttpClient httpClient) : AuthenticationStateProvider
    {
        private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userInfo = await httpClient.GetFromJsonAsync<UserInfoDto>("api/auth/me");
                if (userInfo is null)
                {
                    return Anonymous;
                }

                var claims = new List<Claim> { new(ClaimTypes.Name, userInfo.Username) };
                claims.AddRange(userInfo.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, authenticationType: "jwt-cookie");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                // 401 (eller nätverksfel) betyder att användaren inte är inloggad.
                return Anonymous;
            }
        }

        // Anropas efter inloggning/utloggning för att tvinga Blazor att fråga api/auth/me igen.
        public void NotifyAuthenticationChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
