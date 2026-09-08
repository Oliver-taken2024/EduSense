using System.Collections.Generic;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface IOrganisationService
    {
        Task<IReadOnlyList<OrganisationDto>> GetAllAsync();
        Task<OrganisationDto?> GetByIdAsync(int id);
        Task<OrganisationDto> CreateAsync(OrganisationDto organisation);
        Task<OrganisationDto> UpdateAsync(OrganisationDto organisation);
    }
}
