using System.Collections.Generic;

using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Repositories
{
    public class OrganisationRepository : IOrganisationRepository
    {
        private readonly EduSenseDbContext _context;

        public OrganisationRepository(EduSenseDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<OrganisationModel>> GetAllAsync()
        {
            return await _context.Organisations
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<OrganisationModel?> GetByIdAsync(int id)
        {
            return await _context.Organisations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrganisationModel> CreateAsync(OrganisationModel organisation)
        {
            _context.Organisations.Add(organisation);
            await _context.SaveChangesAsync();
            return organisation;
        }

        public async Task<OrganisationModel> UpdateAsync(OrganisationModel organisation)
        {
            _context.Organisations.Update(organisation);
            await _context.SaveChangesAsync();
            return organisation;
        }
    }
}
