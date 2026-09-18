using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using Moq;

namespace EduSense.BLL.Test;

public class SurveyAiServiceTests
{
    // Testar att GenerateSummaryAsync kastar ett undantag när dispatch inte hittas
    [Fact]
    public async Task GenerateSummaryAsync_throws_when_dispatch_not_found()
    {
        var repoMock = new Mock<ISurveyDispatchRepository>();
        repoMock.Setup(r => r.GetByIdWithResultsAsync(It.IsAny<int>()))
            .ReturnsAsync((SurveyDispatchModel?)null);

        var ollamaMock = new Mock<IOllamaClient>();
        var service = new SurveyAiService(repoMock.Object, ollamaMock.Object);

        var request = new SurveyAiSummaryRequestDto { SurveyDispatchId = 1, PromptType = AiPromptType.TrendSummaryReport };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateSummaryAsync(request, TestContext.Current.CancellationToken));
    }

    // Testar att GenerateSummaryAsync anropar IOllamaClient med korrekt aggregerad text
    [Fact]
    public async Task GenerateSummaryAsync_calls_ollama_with_aggregated_text()
    {
            // Arrange
        var dispatch = new SurveyDispatchModel
        {
            Id = 1,
            Survey = new SurveyModel { Title = "Testenkät" },
            Respondents = new List<RespondentModel>()
        };

        var repoMock = new Mock<ISurveyDispatchRepository>();
        repoMock.Setup(r => r.GetByIdWithResultsAsync(1)).ReturnsAsync(dispatch);

        var ollamaMock = new Mock<IOllamaClient>();
        ollamaMock.Setup(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("AI-genererat svar");

        var service = new SurveyAiService(repoMock.Object, ollamaMock.Object);
        var request = new SurveyAiSummaryRequestDto { SurveyDispatchId = 1, PromptType = AiPromptType.TrendSummaryReport };

        var result = await service.GenerateSummaryAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("AI-genererat svar", result.SummaryText);
        Assert.Equal(AiPromptType.TrendSummaryReport, result.PromptType);
        ollamaMock.Verify(o => o.GenerateAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Testenkät")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Testar att GenerateSummaryAsync stöder alla prompttyper
    [Theory]
    [InlineData(AiPromptType.LowestSatisfactionActionPlan)]
    [InlineData(AiPromptType.TrendSummaryReport)]
    [InlineData(AiPromptType.CorrelationAnalysis)]
    public async Task GenerateSummaryAsync_supports_all_prompt_types(AiPromptType promptType)
    {
        var dispatch = new SurveyDispatchModel
        {
            Id = 1,
            Survey = new SurveyModel { Title = "Testenkät" },
            Respondents = new List<RespondentModel>()
        };

        var repoMock = new Mock<ISurveyDispatchRepository>();
        repoMock.Setup(r => r.GetByIdWithResultsAsync(1)).ReturnsAsync(dispatch);

        var ollamaMock = new Mock<IOllamaClient>();
        ollamaMock.Setup(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("svar");

        var service = new SurveyAiService(repoMock.Object, ollamaMock.Object);
        var request = new SurveyAiSummaryRequestDto { SurveyDispatchId = 1, PromptType = promptType };

        var result = await service.GenerateSummaryAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(promptType, result.PromptType);
    }
}
