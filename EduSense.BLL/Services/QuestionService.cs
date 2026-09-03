using System.Collections.Generic;
using System.Linq;

using EduSense.DAL.Repositories;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<IReadOnlyList<QuestionDto>> GetAllAsync()
        {
            var questions = await _questionRepository.GetAllAsync();

            return questions
                .Select(q => new QuestionDto { Id = q.Id, Text = q.Text })
                .ToList();
        }
    }
}
