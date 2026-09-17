using EduSense.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Repositories
{
    public interface ISurveyDispatchRepository
    {
        Task<IReadOnlyList<SurveyDispatchModel>> GetAllAsync();
        Task<IReadOnlyList<SurveyDispatchModel>> GetAllForSurveyAsync(int surveyId);
        Task<SurveyDispatchModel?> GetByIdAsync(int id);
        Task<SurveyDispatchModel?> GetByIdWithResultsAsync(int id);
        Task<bool> SurveyExistsAsync(int surveyId);
        Task AddAsync(SurveyDispatchModel dispatch);
        Task<bool> DeleteAsync(int id);
    }
}
