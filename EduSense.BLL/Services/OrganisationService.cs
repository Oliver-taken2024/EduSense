using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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

        public async Task<OrganisationDto?> GetByIdAsync(int id)
        {
            var organisation = await _organisationRepository.GetByIdAsync(id);
            if (organisation is null)
            {
                return null;
            }
            return new OrganisationDto { Id = organisation.Id, Name = organisation.Name };
        }

        public async Task<OrganisationDto> CreateAsync(OrganisationDto organisation)
        {
            if (string.IsNullOrWhiteSpace(organisation.Name))
                throw new ValidationException("Namn får inte vara tomt.");

            var created = await _organisationRepository.CreateAsync(new OrganisationModel { Id = organisation.Id, Name = organisation.Name });
            return new OrganisationDto { Id = created.Id, Name = created.Name };
        }

        public async Task<OrganisationDto> UpdateAsync(OrganisationDto organisation)
        {
            if (string.IsNullOrWhiteSpace(organisation.Name))
                throw new ValidationException("Namn får inte vara tomt.");

            var updated = await _organisationRepository.UpdateAsync(new OrganisationModel { Id = organisation.Id, Name = organisation.Name });
            return new OrganisationDto { Id = updated.Id, Name = updated.Name };
        }
    }
}
