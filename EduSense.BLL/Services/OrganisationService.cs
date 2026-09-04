using System.Collections.Generic;
using System.Linq;

using EduSense.DAL.Repositories;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly IOrganisationRepository _organisationRepository;

        public OrganisationService(IOrganisationRepository organisationRepository)
        {
            _organisationRepository = organisationRepository;
        }

        public async Task<IReadOnlyList<OrganisationDto>> GetAllAsync()
        {
            var organisations = await _organisationRepository.GetAllAsync();

            return organisations
                .Select(o => new OrganisationDto { Id = o.Id, Name = o.Name })
                .ToList();
        }
    }
}
