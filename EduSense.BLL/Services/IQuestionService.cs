using EduSense.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduSense.BLL.Services
{
    public interface IQuestionService
    {
        Task<IReadOnlyList<QuestionDto>> GetAllAsync();

        Task<QuestionDto?> GetByIdAsync(int id);

        Task<QuestionDto> CreateAsync(QuestionDto dto);

        Task<QuestionDto?> UpdateAsync(int id, QuestionDto dto);
            
        Task<bool> DeleteAsync(int id);
    }
}
