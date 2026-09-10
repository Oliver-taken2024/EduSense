using System;
using System.Collections.Generic;
using System.Text;
using EduSense.Shared;
using EduSense.DAL.Repositories;
using EduSense.DAL.Models;
using EduSense.BLL.Results;

namespace EduSense.BLL.Services
{
    public class RespondentService : IRespondentService
    {
        private readonly IRespondentRepository _respondentRepository;

        public RespondentService(IRespondentRepository respondentRepository)
        {
            _respondentRepository = respondentRepository;
        }

        public async Task <RespondentResult<RespondentSurveyDto>> GetSurveyByTokenAsync (string token)
        {
            var respondent = await _respondentRepository.GetByTokenAsync(token);

            if (respondent is null)
            {
                return RespondentResult<RespondentSurveyDto>.Failure(RespondentResultStatus.TokenNotFound);
            }

            var survey = respondent.Survey;
            if (survey is null)
            {
                throw new InvalidOperationException("Respondent saknar Survey.");
            }

            if (survey.SurveyExpiryDate < DateTime.UtcNow)
            {
                return RespondentResult<RespondentSurveyDto>.Failure(RespondentResultStatus.SurveyExpired);
            }

            var dto = ToDto(respondent);
            return RespondentResult<RespondentSurveyDto>.Success(dto);
        }

        private static RespondentSurveyDto ToDto(RespondentModel respondent)
        {
            var survey = respondent.Survey!;

            // Slår upp svarat alternativ per fråga via SurveyQuestionId, för förifyllnad vid återupptagning.
            var answeredBySurveyQuestionId = respondent.Responses
                .ToDictionary(r => r.SurveyQuestionId, r => r.QuestionAnswerOptionId);

            return new RespondentSurveyDto
            {
                Title = survey.Title,
                Description = survey.Description,
                IsCompleted = respondent.TokenIsUsed,
                Questions = survey.SurveyQuestions.Select(sq => new RespondentQuestionDto
                {
                    SurveyQuestionId = sq.Id,
                    QuestionText = sq.Question!.Text,
                    AnswerOptions = sq.Question.QuestionAnswerOptions
                        .OrderByDescending(qao => qao.AnswerOption!.Value)
                        .Select(qao => new RespondentAnswerOptionDto
                    {
                        Id = qao.Id,
                        Description = qao.AnswerOption.Description,
                        Value = qao.AnswerOption.Value,
                    }).ToList(),
                    AnsweredQuestionAnswerOptionId = answeredBySurveyQuestionId.TryGetValue(sq.Id, out var selectedId)
                        ? selectedId
                        : null
                }).ToList()
            };
        }
    }
}
