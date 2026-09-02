
using System.ComponentModel.DataAnnotations;


namespace EduSense.DAL.Models
{
    public class AnswerOptionModel
    {
        public int Id { get; set; }

        [Required]
        public required string Description { get; set; }

        public int Value { get; set; }

        public ICollection<QuestionAnswerOptionModel> QuestionAnswerOptions { get; set; } = [];
    }
}
