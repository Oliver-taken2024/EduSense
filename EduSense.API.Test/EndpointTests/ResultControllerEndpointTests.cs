using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;
using Xunit;

namespace EduSense.API.Test.EndpointTests
{
    public class ResultControllerEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ResultControllerEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SaveAnswer_ValidAnswer_ReturnsOk()
        {
            var survey = await _client.GetFromJsonAsync<RespondentSurveyDto>("/api/respondent/token-456");
            var question = survey!.Questions.First();
            var dto = new SaveResultDto
            {
                Token = "token-456",
                SurveyQuestionId = question.SurveyQuestionId,
                QuestionAnswerOptionId = question.AnswerOptions.First().Id
            };

            var response = await _client.PostAsJsonAsync("/api/result", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SaveAnswer_UnknownToken_ReturnsNotFound()
        {
            var survey = await _client.GetFromJsonAsync<RespondentSurveyDto>("/api/respondent/token-456");
            var question = survey!.Questions.First();
            var dto = new SaveResultDto
            {
                Token = "token-999",
                SurveyQuestionId = question.SurveyQuestionId,
                QuestionAnswerOptionId = question.AnswerOptions.First().Id
            };

            var response = await _client.PostAsJsonAsync("/api/result", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Complete_ValidToken_ReturnsOk()
        {
            var response = await _client.PostAsync("/api/result/token-101/complete", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Complete_UnknownToken_ReturnsNotFound()
        {
            var response = await _client.PostAsync("/api/result/token-999/complete", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}