namespace EduSense.Shared
{
    public class SurveyAiSummaryResultDto
    {
        public AiPromptType PromptType { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}