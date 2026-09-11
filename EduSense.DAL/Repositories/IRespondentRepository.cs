using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IRespondentRepository
    {
        Task<RespondentModel?> GetByTokenAsync(string token);

        Task<RespondentModel?> GetTrackedByTokenAsync(string token);

        Task SaveChangesAsync();
    }
}