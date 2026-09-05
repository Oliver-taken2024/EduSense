using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.Shared
{
    public class SurveyDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime SurveyExpiryDate { get; set; }
        public int OrganisationId { get; set; }

        public OrganisationDto? Organisation { get; set; }
        public ICollection<SurveyQuestionDto> SurveyQuestions { get; set; } = [];
    }
}
