
namespace EduSense.DAL.Models
{
    public class SurveyQuestionModel
    {
        public int Id { get; set; }

        public int SurveyId { get; set; }
        public SurveyModel? Survey { get; set; }

        public int QuestionId { get; set; }
        public QuestionModel? Question { get; set; }

        public ICollection<ResponseModel> Responses { get; set; } = [];
    }
}
