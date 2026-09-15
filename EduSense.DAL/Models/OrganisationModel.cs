using System.ComponentModel.DataAnnotations;


namespace EduSense.DAL.Models
{
    public class OrganisationModel
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ICollection<OrganisationUserModel> OrganisationUsers { get; set; } = [];
        public ICollection<SurveyModel> Surveys { get; set; } = [];
    }
}
