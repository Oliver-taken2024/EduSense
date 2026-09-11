using System.Collections.Generic;

namespace EduSense.Shared
{
    public class RespondentQuestionDto
    {
        public int SurveyQuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<RespondentAnswerOptionDto> AnswerOptions { get; set; } = [];

        // Satt om respondenten redan svarat på frågan - används för att
        // förifylla vid återupptagning. Null = obesvarad.
        public int? AnsweredQuestionAnswerOptionId { get; set; }
    }
}
