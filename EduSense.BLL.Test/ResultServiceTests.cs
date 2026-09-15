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
            SurveyDispatchId = 1,
            TokenIsUsed = tokenIsUsed,
            SurveyDispatch = new SurveyDispatchModel
            {
                SurveyId = 1,
                SentByUserId = "user-1",
                ResponseDeadline = expiry ?? DateTime.UtcNow.AddDays(1),
                Survey = new SurveyModel
                {
                    Title = "Enkät",
                    CreatedByUserId = "user-1",
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

        // ----- Aggregeringstester (GetResultForSurveyAsync/GetResultForDispatchAsync/GetOrganisationOverviewAsync) -----

        private static QuestionModel MakeScaleQuestion(int id, string text, CategoryModel? category, int optionCount) => new()
        {
            Id = id,
            Text = text,
            CreatedByUserId = "user-1",
            Category = category,
            QuestionAnswerOptions = Enumerable.Range(1, optionCount)
                .Select(i => new QuestionAnswerOptionModel { Id = i, AnswerOption = new AnswerOptionModel { Value = i, Description = $"Opt{i}" } })
                .ToList()
        };

        private static SurveyQuestionModel MakeSurveyQuestionFor(int id, QuestionModel question) => new()
        {
            Id = id,
            Question = question
        };

        private static ResponseModel MakeAnsweredResponse(SurveyQuestionModel surveyQuestion, int value, string description) => new()
        {
            SurveyQuestion = surveyQuestion,
            QuestionAnswerOption = new QuestionAnswerOptionModel
            {
                AnswerOption = new AnswerOptionModel { Value = value, Description = description }
            }
        };

        private static RespondentModel MakeAggregationRespondent(
            int id, RespondentSegment segment, bool tokenIsUsed, DateTime? tokenUsedAt, params ResponseModel[] responses) => new()
        {
            Id = id,
            Email = $"r{id}@test.se",
            Token = $"token-{id}",
            Segment = segment,
            TokenIsUsed = tokenIsUsed,
            TokenUsedAt = tokenUsedAt,
            Responses = responses.ToList()
        };

        [Fact]
        public async Task GetResultForDispatchAsync_ComputesTotalsResponseRateAndAverage()
        {
            var question = MakeScaleQuestion(1, "Fråga 1", category: null, optionCount: 5);
            var surveyQuestion = MakeSurveyQuestionFor(1, question);

            var completed1 = MakeAggregationRespondent(1, RespondentSegment.Grade7To9, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 5, "Mycket nöjd"));
            var completed2 = MakeAggregationRespondent(2, RespondentSegment.Grade7To9, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 3, "Neutral"));
            var notCompleted = MakeAggregationRespondent(3, RespondentSegment.Grade7To9, false, null);

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { completed1, completed2, notCompleted });

            var result = await _resultService.GetResultForDispatchAsync(10);

            Assert.Equal(2, result.TotalResponses);
            Assert.Equal(200.0 / 3, result.ResponseRate, 2);
            Assert.Equal(4.0, result.AverageScore);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_DetectsNpsQuestionByTenOptionsAndExcludesFromAverage()
        {
            var regularQuestion = MakeScaleQuestion(1, "Trivs du?", category: null, optionCount: 5);
            var npsQuestion = MakeScaleQuestion(2, "Rekommendera?", category: null, optionCount: 10);
            var sqRegular = MakeSurveyQuestionFor(1, regularQuestion);
            var sqNps = MakeSurveyQuestionFor(2, npsQuestion);

            var respondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(sqRegular, 4, "Nöjd"),
                MakeAnsweredResponse(sqNps, 9, "NPS: 9"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { respondent });

            var result = await _resultService.GetResultForDispatchAsync(10);

            Assert.Equal(4.0, result.AverageScore);
            Assert.Equal(100.0, result.Nps);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_CountsQuestionsBelowThresholdAsCritical()
        {
            var badQuestion = MakeScaleQuestion(1, "Dålig fråga", category: null, optionCount: 5);
            var goodQuestion = MakeScaleQuestion(2, "Bra fråga", category: null, optionCount: 5);
            var sqBad = MakeSurveyQuestionFor(1, badQuestion);
            var sqGood = MakeSurveyQuestionFor(2, goodQuestion);

            var respondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(sqBad, 2, "Missnöjd"),
                MakeAnsweredResponse(sqGood, 5, "Mycket nöjd"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { respondent });

            var result = await _resultService.GetResultForDispatchAsync(10);

            Assert.Equal(1, result.CriticalAreasCount);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_GroupsCategoryScoresAndSkipsUncategorized()
        {
            var category = new CategoryModel { Name = "Lärande" };
            var withCategory = MakeScaleQuestion(1, "Fråga A", category, optionCount: 5);
            var withoutCategory = MakeScaleQuestion(2, "Fråga B", category: null, optionCount: 5);
            var sqA = MakeSurveyQuestionFor(1, withCategory);
            var sqB = MakeSurveyQuestionFor(2, withoutCategory);

            var respondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(sqA, 4, "Nöjd"),
                MakeAnsweredResponse(sqB, 2, "Missnöjd"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { respondent });

            var result = await _resultService.GetResultForDispatchAsync(10);

            var categoryScore = Assert.Single(result.CategoryScores);
            Assert.Equal("Lärande", categoryScore.CategoryName);
            Assert.Equal(4.0, categoryScore.AverageScore);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_OrdersTopStrengthsDescendingAndChallengesAscending()
        {
            var high = MakeScaleQuestion(1, "Bäst", category: null, optionCount: 5);
            var mid = MakeScaleQuestion(2, "Mellan", category: null, optionCount: 5);
            var low = MakeScaleQuestion(3, "Sämst", category: null, optionCount: 5);
            var sqHigh = MakeSurveyQuestionFor(1, high);
            var sqMid = MakeSurveyQuestionFor(2, mid);
            var sqLow = MakeSurveyQuestionFor(3, low);

            var respondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(sqHigh, 5, "Mycket nöjd"),
                MakeAnsweredResponse(sqMid, 3, "Neutral"),
                MakeAnsweredResponse(sqLow, 1, "Mycket missnöjd"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { respondent });

            var result = await _resultService.GetResultForDispatchAsync(10);

            Assert.Equal("Bäst", result.TopStrengths[0].QuestionText);
            Assert.Equal("Sämst", result.Challenges[0].QuestionText);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_GroupsSegmentResultsWithPerSegmentNps()
        {
            var npsQuestion = MakeScaleQuestion(1, "Rekommendera?", category: null, optionCount: 10);
            var sqNps = MakeSurveyQuestionFor(1, npsQuestion);

            var gymnasieRespondent = MakeAggregationRespondent(1, RespondentSegment.Gymnasiet, true, DateTime.UtcNow,
                MakeAnsweredResponse(sqNps, 10, "NPS: 10"));
            var personalRespondent = MakeAggregationRespondent(2, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(sqNps, 3, "NPS: 3"));
            var personalNotCompleted = MakeAggregationRespondent(3, RespondentSegment.Personal, false, null);

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { gymnasieRespondent, personalRespondent, personalNotCompleted });

            var result = await _resultService.GetResultForDispatchAsync(10);

            var gymnasieSegment = result.SegmentResults.Single(s => s.Segment == RespondentSegmentDto.Gymnasiet);
            Assert.Equal(100.0, gymnasieSegment.Nps);

            var personalSegment = result.SegmentResults.Single(s => s.Segment == RespondentSegmentDto.Personal);
            Assert.Equal(2, personalSegment.RespondentCount);
            Assert.Equal(50.0, personalSegment.ResponseRate);
            Assert.Equal(-100.0, personalSegment.Nps);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_GroupsTrendByMonthAndCategory()
        {
            var category = new CategoryModel { Name = "Trivsel" };
            var question = MakeScaleQuestion(1, "Fråga", category, optionCount: 5);
            var surveyQuestion = MakeSurveyQuestionFor(1, question);

            var january = MakeAggregationRespondent(1, RespondentSegment.Personal, true, new DateTime(2026, 1, 15),
                MakeAnsweredResponse(surveyQuestion, 4, "Nöjd"));
            var february = MakeAggregationRespondent(2, RespondentSegment.Personal, true, new DateTime(2026, 2, 3),
                MakeAnsweredResponse(surveyQuestion, 2, "Missnöjd"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { january, february });

            var result = await _resultService.GetResultForDispatchAsync(10);

            Assert.Equal(2, result.Trend.Count);
            Assert.Equal(new DateTime(2026, 1, 1), result.Trend[0].PeriodStart);
            Assert.Equal(4.0, result.Trend[0].AverageScore);
            Assert.Equal(new DateTime(2026, 2, 1), result.Trend[1].PeriodStart);
            Assert.Equal(2.0, result.Trend[1].AverageScore);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_ComputesAnswerDistributionPercentages()
        {
            var question = MakeScaleQuestion(1, "Fråga", category: null, optionCount: 5);
            var surveyQuestion = MakeSurveyQuestionFor(1, question);

            var r1 = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 5, "Mycket nöjd"));
            var r2 = MakeAggregationRespondent(2, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 5, "Mycket nöjd"));
            var r3 = MakeAggregationRespondent(3, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 1, "Mycket missnöjd"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel> { r1, r2, r3 });

            var result = await _resultService.GetResultForDispatchAsync(10);

            var nojd = result.AnswerDistribution.Single(a => a.AnswerDescription == "Mycket nöjd");
            Assert.Equal(2, nojd.Count);
            Assert.Equal(200.0 / 3, nojd.Percentage, 2);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_NoRespondents_ReturnsZeroedResultWithoutThrowing()
        {
            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(10))
                .ReturnsAsync(new List<RespondentModel>());

            var result = await _resultService.GetResultForDispatchAsync(10);

            Assert.Equal(0, result.TotalResponses);
            Assert.Equal(0, result.ResponseRate);
            Assert.Equal(0, result.AverageScore);
            Assert.Null(result.Nps);
            Assert.Empty(result.SegmentResults);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_WithPreviousDispatch_ComputesPeriodComparison()
        {
            var question = MakeScaleQuestion(1, "Fråga", category: null, optionCount: 5);
            var surveyQuestion = MakeSurveyQuestionFor(1, question);

            var currentRespondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 5, "Mycket nöjd"));
            var previousRespondent = MakeAggregationRespondent(2, RespondentSegment.Personal, true, DateTime.UtcNow.AddMonths(-1),
                MakeAnsweredResponse(surveyQuestion, 3, "Neutral"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(20))
                .ReturnsAsync(new List<RespondentModel> { currentRespondent });
            _resultRepoMock.Setup(r => r.GetPreviousDispatchIdAsync(20))
                .ReturnsAsync(19);
            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(19))
                .ReturnsAsync(new List<RespondentModel> { previousRespondent });

            var result = await _resultService.GetResultForDispatchAsync(20);

            Assert.Equal(2.0, result.AverageScoreChangePoints);
        }

        [Fact]
        public async Task GetResultForDispatchAsync_NoPreviousDispatch_LeavesComparisonFieldsNull()
        {
            var question = MakeScaleQuestion(1, "Fråga", category: null, optionCount: 5);
            var surveyQuestion = MakeSurveyQuestionFor(1, question);
            var respondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 5, "Mycket nöjd"));

            _resultRepoMock.Setup(r => r.GetRespondentsForDispatchAsync(20))
                .ReturnsAsync(new List<RespondentModel> { respondent });
            _resultRepoMock.Setup(r => r.GetPreviousDispatchIdAsync(20))
                .ReturnsAsync((int?)null);

            var result = await _resultService.GetResultForDispatchAsync(20);

            Assert.Null(result.AverageScoreChangePoints);
            Assert.Null(result.TotalResponsesChangePercent);
        }

        [Fact]
        public async Task GetResultForSurveyAsync_DelegatesToSurveyRepositoryMethod()
        {
            _resultRepoMock.Setup(r => r.GetRespondentsForSurveyAsync(5))
                .ReturnsAsync(new List<RespondentModel>());

            var result = await _resultService.GetResultForSurveyAsync(5);

            Assert.Equal(0, result.TotalResponses);
            _resultRepoMock.Verify(r => r.GetRespondentsForSurveyAsync(5), Times.Once);
        }

        [Fact]
        public async Task GetOrganisationOverviewAsync_ComputesAverageExcludingNpsAndRespondentCount()
        {
            var question = MakeScaleQuestion(1, "Fråga", category: null, optionCount: 5);
            var surveyQuestion = MakeSurveyQuestionFor(1, question);
            var npsQuestion = MakeScaleQuestion(2, "NPS", category: null, optionCount: 10);
            var sqNps = MakeSurveyQuestionFor(2, npsQuestion);

            var respondent = MakeAggregationRespondent(1, RespondentSegment.Personal, true, DateTime.UtcNow,
                MakeAnsweredResponse(surveyQuestion, 4, "Nöjd"),
                MakeAnsweredResponse(sqNps, 9, "NPS: 9"));

            var dispatch = new SurveyDispatchModel { Respondents = [respondent] };
            var survey = new SurveyModel { Title = "Enkät", CreatedByUserId = "user-1", OrganisationId = 1, Dispatches = [dispatch] };
            var organisation = new OrganisationModel { Name = "Skola A", Latitude = 55.6, Longitude = 13.0, Surveys = [survey] };

            _resultRepoMock.Setup(r => r.GetOrganisationsWithResponsesAsync())
                .ReturnsAsync(new List<OrganisationModel> { organisation });

            var result = await _resultService.GetOrganisationOverviewAsync();

            var location = Assert.Single(result);
            Assert.Equal("Skola A", location.OrganisationName);
            Assert.Equal(4.0, location.AverageScore);
            Assert.Equal(1, location.RespondentCount);
        }
    }
}
