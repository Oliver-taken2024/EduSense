using EduSense.BLL.Services;
using EduSense.BLL.Results;
using EduSense.Shared;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using Moq;
using Xunit;

namespace EduSense.BLL.Test
{
    public class ResultServiceTests
    {
        private readonly Mock<IRespondentRepository> _respondentRepoMock = new();
        private readonly Mock<IResultRepository> _resultRepoMock = new();
        private readonly ResultService _resultService;

        public ResultServiceTests()
        {
            _resultService = new ResultService(_respondentRepoMock.Object, _resultRepoMock.Object);
        }

        private static RespondentModel MakeRespondent(bool tokenIsUsed = false, DateTime? expiry = null) => new()
        {
            Id = 1,
            Email = "test@test.se",
            Token = "token-1",
            SurveyId = 1,
            TokenIsUsed = tokenIsUsed,
            Survey = new SurveyModel
            {
                Title = "Enkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = expiry ?? DateTime.UtcNow.AddDays(1),
                SurveyQuestions =
                [
                    new SurveyQuestionModel
                    {
                        Id = 30,
                        Question = new QuestionModel
                        {
                            Text = "Trivs du?",
                            CreatedByUserId = "user-1",
                            QuestionAnswerOptions =
                            [
                                new QuestionAnswerOptionModel { Id = 20, AnswerOption = new AnswerOptionModel { Description = "Ja", Value = 1 } }
                            ]
                        }
                    }
                ]
            }
        };

        [Fact]
        public async Task SaveAnswerAsync_UnknownToken_ReturnsTokenNotFound()
        {
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("bad-token"))
                .ReturnsAsync((RespondentModel?)null);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "bad-token", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 });

            Assert.Equal(ResultSaveStatus.TokenNotFound, result);
        }

        [Fact]
        public async Task SaveAnswerAsync_ExpiredSurvey_ReturnsSurveyExpired()
        {
            var respondent = MakeRespondent(expiry: DateTime.UtcNow.AddDays(-1));
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1")).ReturnsAsync(respondent);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 });

            Assert.Equal(ResultSaveStatus.SurveyExpired, result);
        }

        [Fact]
        public async Task SaveAnswerAsync_AlreadyCompleted_ReturnsAlreadyCompleted()
        {
            var respondent = MakeRespondent(tokenIsUsed: true);
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1")).ReturnsAsync(respondent);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 });

            Assert.Equal(ResultSaveStatus.AlreadyCompleted, result);
        }

        [Fact]
        public async Task SaveAnswerAsync_UnknownSurveyQuestionId_ReturnsInvalidQuestionOrAnswer()
        {
            var respondent = MakeRespondent();
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1")).ReturnsAsync(respondent);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "token-1", SurveyQuestionId = 999, QuestionAnswerOptionId = 20 });

            Assert.Equal(ResultSaveStatus.InvalidQuestionOrAnswer, result);
        }

        [Fact]
        public async Task SaveAnswerAsync_AnswerOptionNotOnQuestion_ReturnsInvalidQuestionOrAnswer()
        {
            var respondent = MakeRespondent();
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1")).ReturnsAsync(respondent);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 999 });

            Assert.Equal(ResultSaveStatus.InvalidQuestionOrAnswer, result);
        }

        [Fact]
        public async Task SaveAnswerAsync_NewAnswer_AddsResponseAndReturnsSuccess()
        {
            var respondent = MakeRespondent();
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1")).ReturnsAsync(respondent);
            _resultRepoMock.Setup(r => r.GetTrackedByRespondentAndQuestionAsync(1, 30))
                .ReturnsAsync((ResponseModel?)null);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 });

            Assert.Equal(ResultSaveStatus.Success, result);
            _resultRepoMock.Verify(r => r.AddAsync(It.Is<ResponseModel>(
                res => res.RespondentId == 1 && res.SurveyQuestionId == 30 && res.QuestionAnswerOptionId == 20)), Times.Once);
        }

        [Fact]
        public async Task SaveAnswerAsync_ExistingAnswer_UpdatesAndReturnsSuccess()
        {
            var respondent = MakeRespondent();
            var existing = new ResponseModel { RespondentId = 1, SurveyQuestionId = 30, QuestionAnswerOptionId = 999 };
            _respondentRepoMock.Setup(r => r.GetByTokenAsync("token-1")).ReturnsAsync(respondent);
            _resultRepoMock.Setup(r => r.GetTrackedByRespondentAndQuestionAsync(1, 30))
                .ReturnsAsync(existing);

            var result = await _resultService.SaveAnswerAsync(new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 });

            Assert.Equal(ResultSaveStatus.Success, result);
            Assert.Equal(20, existing.QuestionAnswerOptionId);
            _resultRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _resultRepoMock.Verify(r => r.AddAsync(It.IsAny<ResponseModel>()), Times.Never);
        }

        [Fact]
        public async Task CompleteAsync_UnknownToken_ReturnsTokenNotFound()
        {
            _respondentRepoMock.Setup(r => r.GetTrackedByTokenAsync("bad-token"))
                .ReturnsAsync((RespondentModel?)null);

            var result = await _resultService.CompleteAsync("bad-token");

            Assert.Equal(ResultSaveStatus.TokenNotFound, result);
        }

        [Fact]
        public async Task CompleteAsync_AlreadyCompleted_ReturnsAlreadyCompleted()
        {
            var respondent = MakeRespondent(tokenIsUsed: true);
            _respondentRepoMock.Setup(r => r.GetTrackedByTokenAsync("token-1")).ReturnsAsync(respondent);

            var result = await _resultService.CompleteAsync("token-1");

            Assert.Equal(ResultSaveStatus.AlreadyCompleted, result);
        }

        [Fact]
        public async Task CompleteAsync_Valid_SetsTokenUsedAndReturnsSuccess()
        {
            var respondent = MakeRespondent();
            _respondentRepoMock.Setup(r => r.GetTrackedByTokenAsync("token-1")).ReturnsAsync(respondent);

            var result = await _resultService.CompleteAsync("token-1");

            Assert.Equal(ResultSaveStatus.Success, result);
            Assert.True(respondent.TokenIsUsed);
            Assert.NotNull(respondent.TokenUsedAt);
            _respondentRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
