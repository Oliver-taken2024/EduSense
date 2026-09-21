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
        public DateTime CreatedAt { get; set; }
        public OrganisationDto? Organisation { get; set; } // härledd, read-only
        public CategoryDto? Category { get; set; } // härledd, read-only

        // Skriv-fält vid skapande: styr om frågan länkas mot NPS-skalan (1-10)
        // istället för standardskalan (1-5). Utan betydelse vid redigering -
        // skalan sätts en gång, vid skapande, och ändras aldrig i efterhand.
        public bool IsNps { get; set; }
    }
}
