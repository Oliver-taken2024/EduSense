

namespace EduSense.DAL.Models
{
    public class OrganisationUserModel
    {
        public int Id { get; set; }

        public int OrganisationId { get; set; }
        public OrganisationModel? Organisation { get; set; }

        public string UserId { get; set; } = null!;
    }
}

