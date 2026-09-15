namespace EduSense.Shared
{
    public class SurveyDispatchSaveDto
    {
        public int SurveyId { get; set; }
        public DateTime ResponseDeadline { get; set; }
        public string SentByUserId { get; set; } = string.Empty;
        public List<RespondentInviteDto> RespondentEmails { get; set; } = [];
    }
}