using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EduSense.API.Test.EndpointTests
{
    public class UsersControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public UsersControllerEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        private async Task<HttpClient> CreateLoggedInClient(string username, string password)
        {
            var client = CreateClient();
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
            {
                Username = username,
                Password = password
            });
            loginResponse.EnsureSuccessStatusCode();
            return client;
        }

        private Task<HttpClient> CreateAdminClientAsync() => CreateLoggedInClient("Admin", "AdminPw123!");
        private Task<HttpClient> CreateAnalystClientAsync() => CreateLoggedInClient("Analytiker", "Analytiker123!");

        [Fact]
        public async Task GetAll_WithoutLogin_ReturnsUnauthorized()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/api/users", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.GetAsync("/api/users", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsAdmin_ReturnsOkWithSeededUsers()
        {
            var client = await CreateAdminClientAsync();

            var response = await client.GetAsync("/api/users", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var users = await response.Content.ReadFromJsonAsync<List<UserListItemDto>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(users);
            Assert.Contains(users, u => u.Email == "admin@edusense.com");
        }

        [Fact]
        public async Task Invite_AsAdmin_ValidRole_ReturnsOk()
        {
            var client = await CreateAdminClientAsync();
            var dto = new InviteUserRequestDto { Email = $"invite-{Guid.NewGuid()}@test.se", Role = "Analyst" };

            var response = await client.PostAsJsonAsync("/api/users/invite", dto, TestContext.Current.CancellationToken );

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Invite_AsAdmin_InvalidRole_ReturnsBadRequest()
        {
            var client = await CreateAdminClientAsync();
            var dto = new InviteUserRequestDto { Email = $"invite-{Guid.NewGuid()}@test.se", Role = "SuperAdmin" };

            var response = await client.PostAsJsonAsync("/api/users/invite", dto, TestContext.Current.CancellationToken );

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_AsAdmin_ExistingUser_ReturnsOk()
        {
            var client = await CreateAdminClientAsync();
            var inviteDto = new InviteUserRequestDto { Email = $"deactivate-{Guid.NewGuid()}@test.se", Role = "Analyst" };
            await client.PostAsJsonAsync("/api/users/invite", inviteDto, TestContext.Current.CancellationToken);

            var users = await client.GetFromJsonAsync<List<UserListItemDto>>("/api/users", cancellationToken: TestContext.Current.CancellationToken);
            var user = users!.First(u => u.Email == inviteDto.Email);

            var response = await client.PostAsync($"/api/users/{user.Id}/deactivate", null, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Activate_AsAdmin_ExistingUser_ReturnsOk()
        {
            var client = await CreateAdminClientAsync();
            var inviteDto = new InviteUserRequestDto { Email = $"activate-{Guid.NewGuid()}@test.se", Role = "Analyst" };
            await client.PostAsJsonAsync("/api/users/invite", inviteDto, TestContext.Current.CancellationToken);

            var users = await client.GetFromJsonAsync<List<UserListItemDto>>("/api/users", cancellationToken: TestContext.Current.CancellationToken);
            var user = users!.First(u => u.Email == inviteDto.Email);

            var response = await client.PostAsync($"/api/users/{user.Id}/activate", null, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_AsAdmin_UnknownId_ReturnsNotFound()
        {
            var client = await CreateAdminClientAsync();

            var response = await client.PostAsync("/api/users/does-not-exist/deactivate", null, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
