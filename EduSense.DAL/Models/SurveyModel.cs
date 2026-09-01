
using System.ComponentModel.DataAnnotations;


namespace EduSense.DAL.Models
{
    public class SurveyModel
    {
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }

        public string CreatedByUserId { get; set; } = null!;

        public DateTime SurveyExpiryDate { get; set; }

        public int OrganisationId { get; set; }
        public OrganisationModel? Organisation { get; set; }

        public ICollection<SurveyQuestionModel> SurveyQuestions { get; set; } = [];
        public ICollection<RespondentModel> Respondents { get; set; } = [];
    }
}
