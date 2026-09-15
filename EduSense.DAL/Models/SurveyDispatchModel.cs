using System.ComponentModel.DataAnnotations;

namespace EduSense.DAL.Models
{
    // Representerar ETT utskick av en survey-mall till en respondentgrupp,
    // med sitt eget slutdatum. Samma SurveyModel kan ha flera dispatches.
    public class SurveyDispatchModel
    {
        public int Id { get; set; }

        public int SurveyId { get; set; }
        public SurveyModel? Survey { get; set; }

        [Required]
        public DateTime ResponseDeadline { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public string SentByUserId { get; set; } = null!;

        public ICollection<RespondentModel> Respondents { get; set; } = [];
    }
}