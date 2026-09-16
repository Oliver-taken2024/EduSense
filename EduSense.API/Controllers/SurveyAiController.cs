using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSense.BLL.Services;
using EduSense.Shared;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyAiController : ControllerBase
    {
        private readonly ISurveyAiService _surveyAiService;
        public SurveyAiController(ISurveyAiService surveyAiService)
        {
            _surveyAiService = surveyAiService;
        }
        // POST /api/surveyai/summary - genererar en sammanfattning av en enkät med hjälp av AI.
        [HttpPost("summary")]
        [Authorize(Policy = "AdminOrAnalyst")]
        public async Task<ActionResult<SurveyAiSummaryResultDto>> GenerateSummary(SurveyAiSummaryRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _surveyAiService.GenerateSummaryAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // T.ex. "Dispatch hittades inte."
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/surveyai/prompt-types - listar tillgängliga promptar för UI:ts väljare.
        [HttpGet("prompt-types")]
        [Authorize(Policy = "AdminOrAnalyst")]
        public ActionResult<IReadOnlyList<AiPromptTypeOptionDto>> GetPromptTypes()
        {
            var options = Enum.GetValues<AiPromptType>()
                .Select(t => new AiPromptTypeOptionDto
                {
                    Value = t,
                    DisplayName = GetDisplayName(t)
                })
                .ToList();

            return Ok(options);
        }

        private static string GetDisplayName(AiPromptType type) => type switch
        {
            AiPromptType.LowestSatisfactionActionPlan => "Handlingsplan för lägst nöjdhet",
            AiPromptType.TrendSummaryReport => "Trendrapport",
            AiPromptType.CorrelationAnalysis => "Sambandsanalys",
            _ => type.ToString()
        };
    }
}