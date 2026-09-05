using System;
using System.Collections.Generic;
using System.Text;
using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IQuestionRepository
    {
        Task<IReadOnlyList<QuestionWithOrganisationModel>> GetAllWithOrganisationAsync();
        Task<QuestionWithOrganisationModel?> GetByIdWithOrganisationAsync(int id);
        Task<QuestionModel?> GetByIdAsync(int id);
        Task<QuestionModel> CreateAsync(QuestionModel question);
        Task<QuestionModel> UpdateAsync(QuestionModel question);
        Task DeleteAsync(QuestionModel question);
    }
}
