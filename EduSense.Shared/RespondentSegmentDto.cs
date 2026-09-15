using System.ComponentModel.DataAnnotations;

namespace EduSense.Shared
{
    public enum RespondentSegmentDto
    {
        [Display(Name = "Grundskola F-6")]
        GradeFTo6,

        [Display(Name = "Årskurs 7-9")]
        Grade7To9,

        [Display(Name = "Gymnasiet")]
        Gymnasiet,

        [Display(Name = "Vuxenutbildning")]
        Vuxenutbildning,

        [Display(Name = "Personal")]
        Personal
    }
}