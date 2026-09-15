using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EduSense.API.Test.EndpointTests
{
    public class AuthControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithTokenExpiry()
        {
            var dto = new LoginRequestDto { Username = "Admin", Password = "AdminPw123!" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CreateTokenResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Me_WithoutLogin_ReturnsUnathorized()
        {
            var response = await _client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Me_AfterLogin_ReturnsOkWithUserInfo()
        {
            var dto = new LoginRequestDto { Username = "Admin", Password = "AdminPw123!" };
            await _client.PostAsJsonAsync("/api/auth/login", dto, TestContext.Current.CancellationToken);

            var response = await _client.GetAsync("/api/auth/me", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>(cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(responseInfo);
            Assert.Equal("Admin", responseInfo.Username);
            Assert.Contains("Admin", responseInfo.Roles);
        }
    }
}
