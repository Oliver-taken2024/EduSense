using System.ComponentModel.DataAnnotations;

namespace EduSense.DAL.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public ICollection<QuestionModel> Questions { get; set; } = [];
    }
}