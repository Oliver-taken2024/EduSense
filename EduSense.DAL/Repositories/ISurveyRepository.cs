using System;
using System.Collections.Generic;
using System.Text;
using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface ISurveyRepository
    {
        Task<IReadOnlyList<SurveyModel>> GetAllAsync();

        Task<SurveyModel?> GetByIdAsync(int id);

        Task<SurveyModel?> GetTrackedByIdAsync(int id);

        Task AddAsync(SurveyModel survey);

        Task SaveChangesAsync();

        Task<bool> DeleteAsync(int id);

        Task<IReadOnlyList<int>> GetExistingQuestionIdsAsync(IEnumerable<int> questionIds);

        Task<bool> TitleExistsAsync(string title, DateTime surveyExpiryDate, int organisationId, int? excludeSurveyId = null);
    }
}
