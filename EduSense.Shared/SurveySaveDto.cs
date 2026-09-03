using System;
using System.Collections.Generic;

namespace EduSense.Shared
{
    public class SurveySaveDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime SurveyExpiryDate { get; set; }
        public int OrganisationId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public List<int> QuestionIds { get; set; } = [];
    }
}
