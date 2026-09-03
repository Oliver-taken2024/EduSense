using System.Collections.Generic;
using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IOrganisationRepository
    {
        Task<IReadOnlyList<OrganisationModel>> GetAllAsync();
    }
}
