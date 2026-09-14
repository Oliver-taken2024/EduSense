using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EduSense.API.Test.EndpointTests
{
    public class SurveyControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SurveyControllerEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // Ny klient per anrop - https krävs för att Secure-cookien (accessToken) ska skickas med.
        private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        private async Task<HttpClient> CreateLoggedInClientAsync(string username, string password)
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

        private Task<HttpClient> CreateAdminClientAsync() => CreateLoggedInClientAsync("Admin", "AdminPw123!");
        private Task<HttpClient> CreateAnalystClientAsync() => CreateLoggedInClientAsync("Analytiker", "AnalystPw123!");

        [Fact]
        public async Task GetAll_WithoutLogin_ReturnsUnauthorized()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/api/survey");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsAnalyst_ReturnsOkWithSeededSurveys()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.GetAsync("/api/survey");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var surveys = await response.Content.ReadFromJsonAsync<List<SurveyDto>>();
            Assert.NotNull(surveys);
            Assert.NotEmpty(surveys);
        }

        [Fact]
        public async Task GetById_AsAnalyst_KnownId_ReturnsOkWithMatchingSurvey()
        {
            var client = await CreateAnalystClientAsync();
            var all = await client.GetFromJsonAsync<List<SurveyDto>>("/api/survey");
            var existing = all!.First();

            var response = await client.GetAsync($"/api/survey/{existing.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var survey = await response.Content.ReadFromJsonAsync<SurveyDto>();
            Assert.Equal(existing.Title, survey!.Title);
        }

        [Fact]
        public async Task GetById_AsAnalyst_UnknownId_ReturnsNotFound()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.GetAsync("/api/survey/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAdmin_ValidSurvey_ReturnsCreatedWithSurvey()
        {
            var client = await CreateAdminClientAsync();
            var all = await client.GetFromJsonAsync<List<SurveyDto>>("/api/survey");
            var organisationId = all!.First().OrganisationId;

            var dto = new SurveySaveDto
            {
                Title = $"Ny enkät {Guid.NewGuid()}",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(10),
                OrganisationId = organisationId,
                CreatedByUserId = "test-user-id"
            };

            var response = await client.PostAsJsonAsync("/api/survey", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<SurveyDto>();
            Assert.Equal(dto.Title, created!.Title);
        }

        [Fact]
        public async Task Create_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();
            var all = await client.GetFromJsonAsync<List<SurveyDto>>("/api/survey");
            var organisationId = all!.First().OrganisationId;

            var dto = new SurveySaveDto
            {
                Title = $"Otillåten enkät {Guid.NewGuid()}",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(10),
                OrganisationId = organisationId,
                CreatedByUserId = "test-user-id"
            };

            var response = await client.PostAsJsonAsync("/api/survey", dto);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsAdmin_DuplicateTitleOrgAndExpiry_ReturnsBadRequest()
        {
            var client = await CreateAdminClientAsync();
            var all = await client.GetFromJsonAsync<List<SurveyDto>>("/api/survey");
            var existing = all!.First();

            var dto = new SurveySaveDto
            {
                Title = existing.Title,
                SurveyExpiryDate = existing.SurveyExpiryDate,
                OrganisationId = existing.OrganisationId,
                CreatedByUserId = "test-user-id"
            };

            var response = await client.PostAsJsonAsync("/api/survey", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_AsAdmin_ExistingSurvey_ReturnsOkWithUpdatedTitle()
        {
            var client = await CreateAdminClientAsync();
            var all = await client.GetFromJsonAsync<List<SurveyDto>>("/api/survey");
            var organisationId = all!.First().OrganisationId;

            var createDto = new SurveySaveDto
            {
                Title = $"Uppdateras {Guid.NewGuid()}",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(5),
                OrganisationId = organisationId,
                CreatedByUserId = "test-user-id"
            };
            var createResponse = await client.PostAsJsonAsync("/api/survey", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SurveyDto>();

            var updateDto = new SurveySaveDto
            {
                Title = $"Uppdaterad titel {Guid.NewGuid()}",
                SurveyExpiryDate = createDto.SurveyExpiryDate,
                OrganisationId = organisationId,
                CreatedByUserId = "test-user-id"
            };

            var response = await client.PutAsJsonAsync($"/api/survey/{created!.Id}", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<SurveyDto>();
            Assert.Equal(updateDto.Title, updated!.Title);
        }

        [Fact]
        public async Task Update_AsAdmin_UnknownId_ReturnsNotFound()
        {
            var client = await CreateAdminClientAsync();

            var dto = new SurveySaveDto
            {
                Title = "Finns inte",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(5),
                OrganisationId = 1,
                CreatedByUserId = "test-user-id"
            };

            var response = await client.PutAsJsonAsync("/api/survey/999999", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAdmin_ExistingSurvey_ReturnsNoContent()
        {
            var client = await CreateAdminClientAsync();
            var all = await client.GetFromJsonAsync<List<SurveyDto>>("/api/survey");
            var organisationId = all!.First().OrganisationId;

            var createDto = new SurveySaveDto
            {
                Title = $"Ta bort mig {Guid.NewGuid()}",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(5),
                OrganisationId = organisationId,
                CreatedByUserId = "test-user-id"
            };
            var createResponse = await client.PostAsJsonAsync("/api/survey", createDto);
            var created = await createResponse.Content.ReadFromJsonAsync<SurveyDto>();

            var response = await client.DeleteAsync($"/api/survey/{created!.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAdmin_UnknownId_ReturnsNotFound()
        {
            var client = await CreateAdminClientAsync();

            var response = await client.DeleteAsync("/api/survey/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.DeleteAsync("/api/survey/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
