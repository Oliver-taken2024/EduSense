using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using EduSense.BLL.Exceptions;

namespace EduSense.BLL.Services
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;

        public SurveyService(ISurveyRepository surveyRepository)
        {
            _surveyRepository = surveyRepository;
        }

        public async Task<IReadOnlyList<SurveyDto>> GetAllAsync()
        {
            var surveys = await _surveyRepository.GetAllAsync();
            return surveys.Select(ToDto).ToList();
        }

        public async Task<SurveyDto?> GetByIdAsync(int id)
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            return survey is null ? null : ToDto(survey);
        }

        public async Task<SurveyDto> CreateAsync(SurveySaveDto dto)
        {
            await ValidateAsync(dto, excludeSurveyId: null);

            var survey = new SurveyModel
            {
                Title = dto.Title,
                SurveyExpiryDate = dto.SurveyExpiryDate,
                OrganisationId = dto.OrganisationId,
                CreatedByUserId = dto.CreatedByUserId,
                SurveyQuestions = dto.QuestionIds.Distinct()
                    .Select(questionId => new SurveyQuestionModel { QuestionId = questionId })
                    .ToList()
            };

            await _surveyRepository.AddAsync(survey);

            var created = await _surveyRepository.GetByIdAsync(survey.Id);
            return ToDto(created!);
        }

        public async Task<SurveyDto?> UpdateAsync(int id, SurveySaveDto dto)
        {
            var survey = await _surveyRepository.GetTrackedByIdAsync(id);
            if (survey is null)
            {
                return null;
            }

            await ValidateAsync(dto, excludeSurveyId: id);

            survey.Title = dto.Title;
            survey.SurveyExpiryDate = dto.SurveyExpiryDate;
            survey.OrganisationId = dto.OrganisationId;

            survey.SurveyQuestions.Clear();
            foreach (var questionId in dto.QuestionIds.Distinct())
            {
                survey.SurveyQuestions.Add(new SurveyQuestionModel { QuestionId = questionId });
            }

            await _surveyRepository.SaveChangesAsync();

            var updated = await _surveyRepository.GetByIdAsync(id);
            return ToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _surveyRepository.DeleteAsync(id);
        }

        private static SurveyDto ToDto(SurveyModel survey)
        {
            return new SurveyDto
            {
                Id = survey.Id,
                Title = survey.Title,
                SurveyExpiryDate = survey.SurveyExpiryDate,
                OrganisationId = survey.OrganisationId,
                Organisation = survey.Organisation is null ? null : new OrganisationDto
                {
                    Id = survey.Organisation.Id,
                    Name = survey.Organisation.Name
                },
                SurveyQuestions = survey.SurveyQuestions.Select(q => new SurveyQuestionDto
                {
                    Id = q.Id,
                    QuestionId = q.QuestionId,
                    QuestionText = q.Question?.Text ?? string.Empty
                }).ToList()
            };
        }

        private async Task ValidateAsync(SurveySaveDto dto, int? excludeSurveyId)
        {
            var errors = new List<string>();

            var distinctQuestionIds = dto.QuestionIds.Distinct().ToList();
            if (distinctQuestionIds.Count > 0)
            {
                var existingIds = await _surveyRepository.GetExistingQuestionIdsAsync(distinctQuestionIds);
                var missingIds = distinctQuestionIds.Except(existingIds).ToList();

                if (missingIds.Count > 0)
                {
                    errors.Add($"Följande frågor finns inte: {string.Join(", ", missingIds)}.");
                }
            }

            var isDuplicate = await _surveyRepository.TitleExistsAsync(
                dto.Title, dto.SurveyExpiryDate, dto.OrganisationId, excludeSurveyId);

            if (isDuplicate)
            {
                errors.Add("Det finns redan en enkät med samma titel, utgångsdatum och organisation.");
            }

            if (errors.Count > 0)
            {
                throw new SurveyValidationException(errors);
            }
        }

    }
}
