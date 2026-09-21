using System.Net;
using System.Net.Http.Json;
using EduSense.API.Test.Helpers;
using EduSense.Shared;

namespace EduSense.API.Test;

public class SurveyAiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    // Använd HttpClient för att skicka HTTP-förfrågningar till testservern och
    // CustomWebApplicationFactory för att konfigurera testmiljön
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SurveyAiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Testar att hämta alla prompt-typer
    [Fact]
    public async Task GetPromptTypes_returns_all_prompt_types_with_display_names()
    {
        _client.DefaultRequestHeaders.Add("Test-Role", "Admin");

        var response = await _client.GetAsync("/api/SurveyAi/prompt-types", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var options = await response.Content.ReadFromJsonAsync<List<AiPromptTypeOptionDto>>(TestContext.Current.CancellationToken);

        Assert.NotNull(options);
        Assert.Equal(2, options!.Count);
        Assert.Contains(options, o => o.Value == AiPromptType.TrendSummaryReport && o.DisplayName == "Trendrapport");
    }

    // Testar att generera en sammanfattning med ett dispatch som inte finns
    [Fact]
    public async Task GenerateSummary_returns_404_when_dispatch_not_found()
    {
        var anonymousClient = _factory.CreateClient();
        anonymousClient.DefaultRequestHeaders.Add("Test-Role", "Admin");

        var request = new SurveyAiSummaryRequestDto
        {
            SurveyDispatchId = 999999,
            PromptType = AiPromptType.TrendSummaryReport
        };

        var response = await anonymousClient.PostAsJsonAsync("/api/SurveyAi/summary", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Testar att generera en sammanfattning med en mockad Ollama-tjänst
    [Fact]
    public async Task GenerateSummary_returns_summary_text_from_mocked_ollama()
    {
        _client.DefaultRequestHeaders.Add("Test-Role", "Admin");
        // Förutsätter att seedad data innehåller ett dispatch med Id = 1
        var request = new SurveyAiSummaryRequestDto
        {
            SurveyDispatchId = 1,
            PromptType = AiPromptType.TrendSummaryReport
        };

        var response = await _client.PostAsJsonAsync("/api/SurveyAi/summary", request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SurveyAiSummaryResultDto>(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Testsvar från AI", result!.SummaryText);
    }

    // Testar att generera en sammanfattning utan autentisering, vilket ska returnera 401 Unauthorized
    [Fact]
    public async Task GenerateSummary_without_auth_returns_401()
    {        
        var anonymousClient = _factory.CreateClient();
        anonymousClient.DefaultRequestHeaders.Add("Test-No-Auth", "true");

        var request = new SurveyAiSummaryRequestDto { SurveyDispatchId = 1, PromptType = AiPromptType.TrendSummaryReport };

        var response = await anonymousClient.PostAsJsonAsync("/api/SurveyAi/summary", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}