using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EduSense.API.Test.EndpointTests
{
    public class QuestionControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public QuestionControllerEndpointTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

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
        private Task<HttpClient> CreateAnalystClientAsync() => CreateLoggedInClientAsync("Analytiker", "Analytiker123!");

        [Fact]
        public async Task GetAll_WithoutLogin_ReturnsUnathorized()
        {
            var unauthorizedClient = CreateClient();

            var response = await unauthorizedClient.GetAsync("/api/question", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        [Fact]
        public async Task GetAll_AsAnalyst_ReturnsOkWithSeededQuestions()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.GetAsync("/api/question", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var questions = await response.Content.ReadFromJsonAsync<List<QuestionDto>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(questions);
            Assert.NotEmpty(questions);
        }
        [Fact]
        public async Task Post_AsAdmin_ValidQuestion_ReturnsCreatedWithQuestion()
        {
            var adminClient = await CreateAdminClientAsync();

            var response = await adminClient.PostAsJsonAsync("/api/question", new QuestionDto
            {
                Text = "Vem är du?",
                CreatedByUserId = "Someone",
                Organisation = new OrganisationDto { Name = "Newton" }
            },
            TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var question = await response.Content.ReadFromJsonAsync<QuestionDto>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(question);
        }

        [Fact]
        public async Task Post_AsAdmin_IgnoresClientSuppliedCreatedByUserId()
        {
            // CreatedByUserId ska alltid komma från den inloggade användarens claims,
            // inte från vad klienten råkar skicka med i request-bodyn.
            var adminClient = await CreateAdminClientAsync();

            var response = await adminClient.PostAsJsonAsync("/api/question", new QuestionDto
            {
                Text = $"Förfalskat user-id {Guid.NewGuid()}",
                CreatedByUserId = "Someone"
            },
            TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var question = await response.Content.ReadFromJsonAsync<QuestionDto>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(question);
            Assert.NotEqual("Someone", question!.CreatedByUserId);
            Assert.False(string.IsNullOrWhiteSpace(question.CreatedByUserId));
        }

        [Fact]
        public async Task Post_AsAdmin_EmptyText_ReturnsBadRequest()
        {
            var client = await CreateAdminClientAsync();
            var dto = new QuestionDto { Text = "", CreatedByUserId = "test-user-id" };

            var response = await client.PostAsJsonAsync("/api/question", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();
            var dto = new QuestionDto { Text = $"Otillåten fråga {Guid.NewGuid()}", CreatedByUserId = "test-user-id" };

            var response = await client.PostAsJsonAsync("/api/question", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Put_AsAdmin_ExistingQuestion_ReturnsOkWithUpdatedText()
        {
            var client = await CreateAdminClientAsync();
            var createDto = new QuestionDto { Text = $"Uppdateras {Guid.NewGuid()}", CreatedByUserId = "test-user-id" };
            var createResponse = await client.PostAsJsonAsync("/api/question", createDto, TestContext.Current.CancellationToken);
            var created = await createResponse.Content.ReadFromJsonAsync<QuestionDto>(cancellationToken: TestContext.Current.CancellationToken);

            var updateDto = new QuestionDto { Text = $"Uppdaterad text {Guid.NewGuid()}", CreatedByUserId = "test-user-id" };

            var response = await client.PutAsJsonAsync($"/api/question/{created!.Id}", updateDto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<QuestionDto>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(updateDto.Text, updated!.Text);
        }

        [Fact]
        public async Task Put_AsAdmin_UnknownId_ReturnsNotFound()
        {
            var client = await CreateAdminClientAsync();
            var dto = new QuestionDto { Text = "Finns inte", CreatedByUserId = "test-user-id" };

            var response = await client.PutAsJsonAsync("/api/question/999999", dto, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAdmin_ExistingQuestion_ReturnsNoContent()
        {
            var client = await CreateAdminClientAsync();
            var createDto = new QuestionDto { Text = $"Ta bort mig {Guid.NewGuid()}", CreatedByUserId = "test-user-id" };
            var createResponse = await client.PostAsJsonAsync("/api/question", createDto, TestContext.Current.CancellationToken);
            var created = await createResponse.Content.ReadFromJsonAsync<QuestionDto>(cancellationToken: TestContext.Current.CancellationToken);

            var response = await client.DeleteAsync($"/api/question/{created!.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAdmin_UnknownId_ReturnsNotFound()
        {
            var client = await CreateAdminClientAsync();

            var response = await client.DeleteAsync("/api/question/999999", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsAnalyst_ReturnsForbidden()
        {
            var client = await CreateAnalystClientAsync();

            var response = await client.DeleteAsync("/api/question/1", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
