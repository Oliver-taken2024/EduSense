
using System.ComponentModel.DataAnnotations;



namespace EduSense.DAL.Models
{
    public class QuestionModel
    {
        public int Id { get; set; }

        [Required]
        public required string Text { get; set; }

        public string CreatedByUserId { get; set; } = null!;

        public ICollection<SurveyQuestionModel> SurveyQuestions { get; set; } = [];
        public ICollection<QuestionAnswerOptionModel> QuestionAnswerOptions { get; set; } = [];
    }
}
