namespace EduSense.Shared
{
    public class SurveyAiSummaryRequestDto
    {
        public int SurveyDispatchId { get; set; }
        public AiPromptType PromptType { get; set; }
    }
}