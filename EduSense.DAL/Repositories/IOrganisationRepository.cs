using System.Collections.Generic;
using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IOrganisationRepository
    {
        Task<IReadOnlyList<OrganisationModel>> GetAllAsync();

        Task<OrganisationModel?> GetByIdAsync(int id);
        Task<OrganisationModel> CreateAsync(OrganisationModel organisation);
        Task<OrganisationModel> UpdateAsync(OrganisationModel organisation);
    }
}
