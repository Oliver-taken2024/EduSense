using System.Collections.Generic;

namespace EduSense.Shared
{
    public class RespondentSurveyDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // True om token redan använts för ett fullständigt inskick -
        // UI ska då visa "redan besvarad" istället för frågeflödet.
        public bool IsCompleted { get; set; }

        public List<RespondentQuestionDto> Questions { get; set; } = [];
    }
}
