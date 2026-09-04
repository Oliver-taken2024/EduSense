using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
            var questions = await _questionRepository.GetAllWithOrganisationAsync();

            return questions
                .Select(q => new QuestionDto 
                { 
                    Id = q.Question.Id, 
                    Text = q.Question.Text, 
                    CreatedByUserId = q.Question.CreatedByUserId,
                    Organisation = q.Organisation is null
                ? null
                : new OrganisationDto { Id = q.Organisation.Id, Name = q.Organisation.Name }
                })
        .ToList();
        }

        public async Task<QuestionDto?> GetByIdAsync(int id)
        {
            var question = await _questionRepository.GetByIdAsync(id);
            if (question is null)
            {
                return null;
            }

            return new QuestionDto
            {
                Id = question.Question.Id,
                Text = question.Question.Text,
                CreatedByUserId = question.Question.CreatedByUserId,
                Organisation = question.Organisation is null
                    ? null
                    : new OrganisationDto { Id = question.Organisation.Id, Name = question.Organisation.Name }
            };
        }

        public async Task<QuestionDto> CreateAsync(QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                throw new ValidationException("Frågetext får inte vara tom.");
            }

            var question = new QuestionModel
            {
                Text = dto.Text,
                CreatedByUserId = dto.CreatedByUserId
            };

            await _questionRepository.CreateAsync(question);

            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                CreatedByUserId = question.CreatedByUserId,
                Organisation = dto.Organisation
            };
        }

        public async Task<QuestionDto?> UpdateAsync(int id, QuestionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
            {
                throw new ValidationException("Frågetext får inte vara tom.");
            }

            var question = await _questionRepository.GetByIdAsync(id);
            if (question is null)
            {
                return null;
            }

            question.Text = dto.Text;
            await _questionRepository.UpdateAsync(question);

            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                CreatedByUserId = question.CreatedByUserId,
                Organisation = dto.Organisation
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var question = await _questionRepository.GetByIdWithOrganisationAsync(id);
            if (question is null)
            {
                return false;
            }

            await _questionRepository.DeleteAsync(question);
            return true;
        }
    }
}
