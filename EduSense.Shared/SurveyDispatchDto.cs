namespace EduSense.Shared
{
    public class SurveyDispatchDto
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string? SurveyTitle { get; set; }
        public DateTime ResponseDeadline { get; set; }
        public DateTime SentAt { get; set; }
        public string SentByUserId { get; set; } = string.Empty;
        public int RespondentCount { get; set; }
        public int ResponseCount { get; set; }
    }
}