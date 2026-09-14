using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using EduSense.DAL.Models;
using EduSense.DAL.Data;

namespace EduSense.API.Test.EndpointTests
{
    public class OrganisationControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public OrganisationControllerEndpointTests(CustomWebApplicationFactory factory)
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
        private Task<HttpClient> CreateAnalystClientAsync() => CreateLoggedInClient("Analyst", "AnalystPw123!");

        [Fact]
        public async Task GetAll_WithoutLogin_ReturnsUnauthorized()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/api/organisation", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsAnalyst_ReturnsOkWithSeededOrganisations()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.GetAsync("/api/organisation", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var organisations = await response.Content.ReadFromJsonAsync<List<OrganisationDto>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(organisations);
            Assert.NotEmpty(organisations);
        }

        [Fact]
        public async Task Post_AsAdmin_ValidName_ReturnsCreatedWithOrganisation()
        {
            var client = await CreateAdminClientAsync();
            var dto = new OrganisationDto { Name = $"Ny organisation {Guid.NewGuid()}" };

            var response = await client.PostAsJsonAsync("/api/organisation", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<OrganisationDto>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(dto.Name, created!.Name);
        }

        [Fact]
        public async Task Post_AsAdmin_EmptyName_ReturnsBadRequest()
        {
            var client = await CreateAdminClientAsync();

            var dto = new OrganisationDto { Name = "" };

            var response = await client.PostAsJsonAsync("/api/organisation", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();
            var dto = new OrganisationDto { Name = "Newton" };

            var response = await client.PostAsJsonAsync("/api/organisation", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Put_AsAdmin_ExistingOrganisation_ReturnsOkWithUpdatedName()
        {

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EduSenseDbContext>();
            var organisation = new OrganisationModel { Name = "Newton" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var client = await CreateAdminClientAsync();
            var dto = new OrganisationDto { Id = organisation.Id, Name= "Malmö Universitet" };

            var response = await client.PutAsJsonAsync($"/api/organisation/{organisation.Id}", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<OrganisationDto>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(dto.Name, updated!.Name);
        }

        [Fact]
        public async Task Put_AsAdmin_IdMismatch_ReturnsBadRequest()
        {
            var client = await CreateAdminClientAsync();
            var dto = new OrganisationDto { Id = 2, Name = "Fel id" };

            var response = await client.PutAsJsonAsync("/api/organisation/1", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();
            var dto = new OrganisationDto { Id = 1, Name = "Otillåten uppdatering" };

            var response = await client.PutAsJsonAsync("/api/organisation/1", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
