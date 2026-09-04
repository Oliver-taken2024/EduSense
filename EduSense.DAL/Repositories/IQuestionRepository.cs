using System;
using System.Collections.Generic;
using System.Text;
using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IQuestionRepository
    {
        Task<IReadOnlyList<QuestionModel>> GetAllAsync();
    }
}
