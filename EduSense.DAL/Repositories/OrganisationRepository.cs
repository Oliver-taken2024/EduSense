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
    }
}
