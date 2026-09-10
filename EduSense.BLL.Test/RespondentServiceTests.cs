using EduSense.BLL.Services;
using EduSense.BLL.Results;
using EduSense.Shared;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using Moq;
using Xunit;

namespace EduSense.API.Controllers
{
    public class RespondentServiceTests
    {
        private readonly Mock<IRespondentRepository> _respondentRepoMock = new();
        private readonly RespondentService _respondentService;

        public RespondentServiceTests()
        {
            _respondentService = new RespondentService(_respondentRepoMock.Object);
        }

        [Fact]
        public async Task GetSurveyByTokenAsync_UnknownToken_ReturnsTokenNotFound()
        {
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("bad-token"))
                .ReturnsAsync((RespondentModel?)null);

            var result = await _respondentService.GetSurveyByTokenAsync("bad-token");

            Assert.Equal(RespondentResultStatus.TokenNotFound, result.Status);
        }


        [Fact]
        public async Task GetSurveyByTokenAsync_ExpiredSurvey_ReturnsSurveyExpired()
        {
            var respondent = new RespondentModel
            {
                Email = "test@test.se",
                Token = "token-1",
                SurveyId = 1,
                Survey = new SurveyModel
                {
                    Title = "Enkät",
                    CreatedByUserId = "user-1",
                    SurveyExpiryDate = DateTime.UtcNow.AddDays(-1)
                }
            };

            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1"))
                .ReturnsAsync(respondent);

            var result = await _respondentService.GetSurveyByTokenAsync("token-1");

            Assert.Equal(RespondentResultStatus.SurveyExpired, result.Status);
        }

        [Fact]
        public async Task GetSurveyByTokenAsync_ValidToken_ReturnsMappedSurvey()
        {
            var answerOption = new AnswerOptionModel { Id = 10, Description = "Ja", Value = 1 };
            var questionAnswerOption = new QuestionAnswerOptionModel { Id = 20, AnswerOption = answerOption };
            var question = new QuestionModel
            {
                Text = "Trivs du?",
                CreatedByUserId = "user-1",
                QuestionAnswerOptions = [questionAnswerOption]
            };
            var surveyQuestion = new SurveyQuestionModel { Id = 30, Question = question };

            var respondent = new RespondentModel
            {
                Email = "test@test.se",
                Token = "token-1",
                SurveyId = 1,
                TokenIsUsed = true,
                Survey = new SurveyModel
                {
                    Title = "Enkät",
                    Description = "Beskrivning",
                    CreatedByUserId = "user-1",
                    SurveyExpiryDate = DateTime.UtcNow.AddDays(1),
                    SurveyQuestions = [surveyQuestion]
                },
                Responses = [new ResponseModel { SurveyQuestionId = 30, QuestionAnswerOptionId = 20 }]
            };

            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1"))
                .ReturnsAsync(respondent);

            var result = await _respondentService.GetSurveyByTokenAsync("token-1");

            Assert.Equal(RespondentResultStatus.Success, result.Status);
            Assert.Equal("Enkät", result.Value!.Title);
            Assert.True(result.Value.IsCompleted);

            var mappedQuestion = Assert.Single(result.Value.Questions);
            Assert.Equal("Trivs du?", mappedQuestion.QuestionText);
            Assert.Equal(20, mappedQuestion.AnsweredQuestionAnswerOptionId);

            var mappedOption = Assert.Single(mappedQuestion.AnswerOptions);
            Assert.Equal("Ja", mappedOption.Description);
            Assert.Equal(1, mappedOption.Value);
        }
    }
}
