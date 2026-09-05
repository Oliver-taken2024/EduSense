using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Models
{
    public class QuestionWithOrganisationModel
    {
        public required QuestionModel Question { get; set; }
        public OrganisationModel? Organisation { get; set; }
    }
}