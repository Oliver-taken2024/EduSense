

namespace EduSense.DAL.Models
{
    public class QuestionAnswerOptionModel
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
        public QuestionModel? Question { get; set; }

        public int AnswerOptionId { get; set; }
        public AnswerOptionModel? AnswerOption { get; set; }

        public ICollection<ResponseModel> Responses { get; set; } = [];
    }
}
