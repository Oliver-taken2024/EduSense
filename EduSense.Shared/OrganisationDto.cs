using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EduSense.Shared
{
    public class OrganisationDto
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}
