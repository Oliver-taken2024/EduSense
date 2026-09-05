using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.Shared
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string CreatedByUserId { get; set; } = string.Empty;
        public OrganisationDto? Organisation { get; set; } // härledd, read-only
    }
}
