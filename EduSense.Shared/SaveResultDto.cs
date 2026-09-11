namespace EduSense.Shared
{
    public class SaveResultDto
    {
        public string Token { get; set; } = string.Empty;
        public int SurveyQuestionId { get; set; }
        public int QuestionAnswerOptionId { get; set; }
    }
}
