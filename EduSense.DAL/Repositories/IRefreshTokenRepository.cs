using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshTokenModel refreshToken);
        Task<RefreshTokenModel?> GetByTokenAsync(string token);
        Task RemoveAsync(RefreshTokenModel refreshToken);
    }
}
