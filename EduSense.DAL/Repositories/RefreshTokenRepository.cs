using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly EduSenseUserDbContext _context;

        public RefreshTokenRepository(EduSenseUserDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshTokenModel refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshTokenModel?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RemoveAsync(RefreshTokenModel refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
