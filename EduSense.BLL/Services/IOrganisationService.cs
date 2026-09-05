using System.Collections.Generic;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface IOrganisationService
    {
        Task<IReadOnlyList<OrganisationDto>> GetAllAsync();
    }
}
