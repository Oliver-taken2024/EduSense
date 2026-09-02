

namespace EduSense.DAL.Models
{
    public class ResponseModel
    {
        public int Id { get; set; }

        public int RespondentId { get; set; }
        public RespondentModel? Respondent { get; set; }

        public int SurveyQuestionId { get; set; }
        public SurveyQuestionModel? SurveyQuestion { get; set; }

        public int QuestionAnswerOptionId { get; set; }
        public QuestionAnswerOptionModel? QuestionAnswerOption { get; set; }
    }
}
