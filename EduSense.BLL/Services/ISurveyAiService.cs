using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface ISurveyAiService
    {
        Task<SurveyAiSummaryResultDto> GenerateSummaryAsync(SurveyAiSummaryRequestDto request, CancellationToken cancellationToken = default);
    }
}