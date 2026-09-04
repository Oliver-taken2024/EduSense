using System.Collections.Generic;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface IQuestionService
    {
        Task<IReadOnlyList<QuestionDto>> GetAllAsync();
    }
}
