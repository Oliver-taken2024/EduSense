
using System.ComponentModel.DataAnnotations;


namespace EduSense.DAL.Models
{

    public class RespondentModel
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }

        public int SurveyId { get; set; }
        public SurveyModel? Survey { get; set; }

        public bool TokenIsUsed { get; set; }

        public DateTime? TokenUsedAt { get; set; }

        public ICollection<ResponseModel> Responses { get; set; } = [];
    }
}
    
