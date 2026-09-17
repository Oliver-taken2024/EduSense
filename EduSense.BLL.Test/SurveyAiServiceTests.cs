using EduSense.BLL.Services;
using EduSense.Shared;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using Moq;
using Xunit;

namespace EduSense.BLL.Test
{
    public class SurveyAiServiceTests
    {
        private readonly Mock<ISurveyDispatchRepository> _dispatchRepositoryMock = new();
        private readonly Mock<IOllamaClient> _ollamaClientMock = new();
        private readonly SurveyAiService _aiService;

        public SurveyAiServiceTests()
        {
            _aiService = new SurveyAiService(_dispatchRepositoryMock.Object, _ollamaClientMock.Object);
        }

        /// <summary>
        /// Creates a sample SurveyDispatchModel with responses for testing
        /// </summary>
        private static SurveyDispatchModel CreateSampleDispatch(int dispatchId = 1)
        {
            var survey = new SurveyModel
            {
                Id = 1,
                Title = "Test Survey",
                CreatedByUserId = "user-1",
                SurveyQuestions = new List<SurveyQuestionModel>
                {
                    new()
                    {
                        Id = 1,
                        Question = new QuestionModel
                        {
                            Id = 1,
                            Text = "How satisfied are you?",
                            CreatedByUserId = "user-1",
                            QuestionAnswerOptions = new List<QuestionAnswerOptionModel>
                            {
                                new() { Id = 1, AnswerOption = new AnswerOptionModel { Id = 1, Value = 1, Description = "Very dissatisfied" } },
                                new() { Id = 2, AnswerOption = new AnswerOptionModel { Id = 2, Value = 5, Description = "Very satisfied" } }
                            }
                        }
                    },
                    new()
                    {
                        Id = 2,
                        Question = new QuestionModel
                        {
                            Id = 2,
                            Text = "Quality of service?",
                            CreatedByUserId = "user-1",
                            QuestionAnswerOptions = new List<QuestionAnswerOptionModel>
                            {
                                new() { Id = 3, AnswerOption = new AnswerOptionModel { Id = 3, Value = 2, Description = "Poor" } },
                                new() { Id = 4, AnswerOption = new AnswerOptionModel { Id = 4, Value = 4, Description = "Good" } }
                            }
                        }
                    }
                }
            };

            var respondent1 = new RespondentModel
            {
                Id = 1,
                Email = "respondent1@test.se",
                Token = "token-1",
                TokenIsUsed = true,
                Segment = RespondentSegment.Personal,
                SurveyDispatchId = dispatchId,
                Responses = new List<ResponseModel>
                {
                    new()
                    {
                        Id = 1,
                        RespondentId = 1,
                        SurveyQuestionId = 1,
                        QuestionAnswerOptionId = 2,
                        SurveyQuestion = survey.SurveyQuestions.ElementAt(0),
                        QuestionAnswerOption = survey.SurveyQuestions.ElementAt(0).Question.QuestionAnswerOptions.ElementAt(1)
                    },
                    new()
                    {
                        Id = 2,
                        RespondentId = 1,
                        SurveyQuestionId = 2,
                        QuestionAnswerOptionId = 4,
                        SurveyQuestion = survey.SurveyQuestions.ElementAt(1),
                        QuestionAnswerOption = survey.SurveyQuestions.ElementAt(1).Question.QuestionAnswerOptions.ElementAt(1)
                    }
                }
            };

            var respondent2 = new RespondentModel
            {
                Id = 2,
                Email = "respondent2@test.se",
                Token = "token-2",
                TokenIsUsed = true,
                Segment = RespondentSegment.GradeFTo6,
                SurveyDispatchId = dispatchId,
                Responses = new List<ResponseModel>
                {
                    new()
                    {
                        Id = 3,
                        RespondentId = 2,
                        SurveyQuestionId = 1,
                        QuestionAnswerOptionId = 1,
                        SurveyQuestion = survey.SurveyQuestions.ElementAt(0),
                        QuestionAnswerOption = survey.SurveyQuestions.ElementAt(0).Question.QuestionAnswerOptions.ElementAt(0)
                    },
                    new()
                    {
                        Id = 4,
                        RespondentId = 2,
                        SurveyQuestionId = 2,
                        QuestionAnswerOptionId = 3,
                        SurveyQuestion = survey.SurveyQuestions.ElementAt(1),
                        QuestionAnswerOption = survey.SurveyQuestions.ElementAt(1).Question.QuestionAnswerOptions.ElementAt(0)
                    }
                }
            };

            return new SurveyDispatchModel
            {
                Id = dispatchId,
                SurveyId = 1,
                Survey = survey,
                SentByUserId = "user-1",
                SentAt = DateTime.UtcNow.AddDays(-7),
                ResponseDeadline = DateTime.UtcNow.AddDays(7),
                Respondents = new List<RespondentModel> { respondent1, respondent2 }
            };
        }

        // ============ HAPPY PATH TESTS ============

        [Fact]
        public async Task GenerateSummaryAsync_WithValidLowestSatisfactionRequest_ReturnsAiResult()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Action plan: 1) Improve X, 2) Focus on Y, 3) Enhance Z");

            // Act
            var result = await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AiPromptType.LowestSatisfactionActionPlan, result.PromptType);
            Assert.Equal("Action plan: 1) Improve X, 2) Focus on Y, 3) Enhance Z", result.SummaryText);
            Assert.NotEqual(default, result.GeneratedAt);
        }

        [Fact]
        public async Task GenerateSummaryAsync_WithValidTrendSummaryRequest_ReturnsAiResult()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(2);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 2,
                PromptType = AiPromptType.TrendSummaryReport
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(2))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Trend summary: Overall satisfaction is increasing with focus on service quality");

            // Act
            var result = await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AiPromptType.TrendSummaryReport, result.PromptType);
            Assert.Contains("Trend summary", result.SummaryText);
        }

        [Fact]
        public async Task GenerateSummaryAsync_WithValidCorrelationAnalysisRequest_ReturnsAiResult()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(3);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 3,
                PromptType = AiPromptType.CorrelationAnalysis
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(3))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Correlation found: Questions 1 and 2 show moderate positive correlation");

            // Act
            var result = await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AiPromptType.CorrelationAnalysis, result.PromptType);
            Assert.Contains("Correlation", result.SummaryText);
        }

        // ============ ERROR HANDLING TESTS ============

        [Fact]
        public async Task GenerateSummaryAsync_WithNonExistentDispatch_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 999,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(999))
                .ReturnsAsync((SurveyDispatchModel?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _aiService.GenerateSummaryAsync(request)
            );

            Assert.Contains("Dispatch hittades inte", exception.Message);
        }

        [Fact]
        public async Task GenerateSummaryAsync_WithInvalidPromptType_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = (AiPromptType)999 // Invalid enum value
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _aiService.GenerateSummaryAsync(request)
            );
        }

        // ============ EDGE CASE TESTS ============

        [Fact]
        public async Task GenerateSummaryAsync_WithNoResponses_StillProcessesSuccessfully()
        {
            // Arrange
            var dispatch = new SurveyDispatchModel
            {
                Id = 1,
                SurveyId = 1,
                Survey = new SurveyModel
                {
                    Id = 1,
                    Title = "Empty Survey",
                    CreatedByUserId = "user-1",
                    SurveyQuestions = new List<SurveyQuestionModel>()
                },
                SentByUserId = "user-1",
                SentAt = DateTime.UtcNow.AddDays(-7),
                ResponseDeadline = DateTime.UtcNow.AddDays(7),
                Respondents = new List<RespondentModel>() // Empty responses
            };

            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.TrendSummaryReport
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("No data available for analysis");

            // Act
            var result = await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SummaryText);
        }

        [Fact]
        public async Task GenerateSummaryAsync_WithCancellationToken_PassesTokenToOllamaClient()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            var cancellationToken = new CancellationToken();

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync("Test response");

            // Act
            var result = await _aiService.GenerateSummaryAsync(request, cancellationToken);

            // Assert
            Assert.NotNull(result);
            _ollamaClientMock.Verify(
                c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken),
                Times.Once
            );
        }

        // ============ INTEGRATION TESTS ============

        [Fact]
        public async Task GenerateSummaryAsync_VerifiesCorrectSystemPromptUsedForLowestSatisfaction()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            string? capturedSystemPrompt = null;

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback((string systemPrompt, string input, CancellationToken ct) => 
                {
                    capturedSystemPrompt = systemPrompt;
                })
                .ReturnsAsync("Response");

            // Act
            await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(capturedSystemPrompt);
            Assert.Contains("tre fr", capturedSystemPrompt); // Swedish: "tre frågor" (three questions)
            Assert.Contains("handlingsplan", capturedSystemPrompt); // Swedish: action plan
        }

        [Fact]
        public async Task GenerateSummaryAsync_IncludesDispatchDataInAggregatedInput()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            string? capturedInput = null;

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback((string systemPrompt, string input, CancellationToken ct) =>
                {
                    capturedInput = input;
                })
                .ReturnsAsync("Response");

            // Act
            await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(capturedInput);
            Assert.Contains("Test Survey", capturedInput); // Survey title should be in the input
            Assert.Contains("How satisfied are you?", capturedInput); // Question text should be included
        }

        [Theory]
        [InlineData(AiPromptType.LowestSatisfactionActionPlan)]
        [InlineData(AiPromptType.TrendSummaryReport)]
        [InlineData(AiPromptType.CorrelationAnalysis)]
        public async Task GenerateSummaryAsync_AllPromptTypesProcessSuccessfully(AiPromptType promptType)
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = promptType
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync($"Response for {promptType}");

            // Act
            var result = await _aiService.GenerateSummaryAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(promptType, result.PromptType);
            Assert.Contains($"Response for {promptType}", result.SummaryText);
        }

        // ============ MOCK VERIFICATION TESTS ============

        [Fact]
        public async Task GenerateSummaryAsync_CallsDispatchRepositoryExactlyOnce()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            await _aiService.GenerateSummaryAsync(request);

            // Assert
            _dispatchRepositoryMock.Verify(
                r => r.GetByIdWithResultsAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateSummaryAsync_CallsOllamaClientExactlyOnce()
        {
            // Arrange
            var dispatch = CreateSampleDispatch(1);
            var request = new SurveyAiSummaryRequestDto
            {
                SurveyDispatchId = 1,
                PromptType = AiPromptType.LowestSatisfactionActionPlan
            };

            _dispatchRepositoryMock
                .Setup(r => r.GetByIdWithResultsAsync(1))
                .ReturnsAsync(dispatch);

            _ollamaClientMock
                .Setup(c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

            // Act
            await _aiService.GenerateSummaryAsync(request);

            // Assert
            _ollamaClientMock.Verify(
                c => c.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
