using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.Shared
{
    public class SurveyQuestionDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }
}
