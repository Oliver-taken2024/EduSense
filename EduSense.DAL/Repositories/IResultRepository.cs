using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IResultRepository
    {
        Task<ResponseModel?> GetTrackedByRespondentAndQuestionAsync(int respondentId, int surveyQuestionId);

        Task AddAsync(ResponseModel response);

        Task SaveChangesAsync();
    }
}
