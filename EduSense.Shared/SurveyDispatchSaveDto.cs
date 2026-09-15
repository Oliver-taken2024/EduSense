namespace EduSense.Shared
{
    public class SurveyDispatchSaveDto
    {
        public int SurveyId { get; set; }
        public DateTime ResponseDeadline { get; set; }
        public string SentByUserId { get; set; } = string.Empty;

        // E-postadresser till respondenterna som ska bjudas in i detta utskick
        public List<string> RespondentEmails { get; set; } = [];
    }
}