using System.ComponentModel.DataAnnotations;

namespace EduSense.DAL.Models
{
    // Representerar en survey-MALL: titel, beskrivning och vilka frågor den innehåller.
    // Har inget eget slutdatum eller egna respondenter - det hanteras per utskick (SurveyDispatchModel).
    public class SurveyModel
    {
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }

        public string CreatedByUserId { get; set; } = null!;

        public int OrganisationId { get; set; }
        public OrganisationModel? Organisation { get; set; }

        public ICollection<SurveyQuestionModel> SurveyQuestions { get; set; } = [];
        public ICollection<SurveyDispatchModel> Dispatches { get; set; } = [];
    }
}