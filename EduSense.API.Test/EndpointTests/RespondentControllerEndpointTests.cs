using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Xunit;

namespace EduSense.API.Test.EndpointTests
{
    // Riktiga HTTP-anrop genom hela pipen (routing/auth/controller), till skillnad
    // från RespondentControllerTests som mockar IRespondentService direkt.
    public class RespondentControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RespondentControllerEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetSurveyByToken_KnownToken_ReturnsOkWithSurvey()
        {
            // Arrange
            const string token = "token-123";

            // Act
            var response = await _client.GetAsync($"/api/respondent/{token}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dto = await response.Content.ReadFromJsonAsync<RespondentSurveyDto>();
            Assert.NotNull(dto);
        }

        [Fact]
        public async Task GetSurveyByToken_UnknownToken_ReturnsNotFound()
        {
            // Arrange
            const string token = "does-not-exist";

            // Act
            var response = await _client.GetAsync($"/api/respondent/{token}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetSurveyByToken_ExpiredSurvey_ReturnsBadRequest()
        {
            // Arrange
            const string token = "token-expired";

            // Act
            var response = await _client.GetAsync($"/api/respondent/{token}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

    }
   
}
