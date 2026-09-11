using System.Linq;
using EduSense.BLL.Results;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public class ResultService : IResultService
    {
        private readonly IRespondentRepository _respondentRepository;
        private readonly IResultRepository _resultRepository;

        public ResultService(IRespondentRepository respondentRepository, IResultRepository resultRepository)
        {
            _respondentRepository = respondentRepository;
            _resultRepository = resultRepository;
        }

        public async Task<ResultSaveStatus> SaveAnswerAsync(SaveResultDto dto)
        {
            var respondent = await _respondentRepository.GetByTokenAsync(dto.Token);
            if (respondent is null)
            {
                return ResultSaveStatus.TokenNotFound;
            }

            var survey = respondent.Survey;
            if (survey is null)
            {
                throw new InvalidOperationException("Respondent saknar Survey.");
            }

            if (survey.SurveyExpiryDate < DateTime.UtcNow)
            {
                return ResultSaveStatus.SurveyExpired;
            }

            if (respondent.TokenIsUsed)
            {
                return ResultSaveStatus.AlreadyCompleted;
            }

            // Boundary-validering: SurveyQuestionId/QuestionAnswerOptionId kommer från klienten
            // och måste faktiskt tillhöra den här enkäten/frågan.
            var surveyQuestion = survey.SurveyQuestions.FirstOrDefault(sq => sq.Id == dto.SurveyQuestionId);
            var isValidAnswer = surveyQuestion?.Question?.QuestionAnswerOptions
                .Any(qao => qao.Id == dto.QuestionAnswerOptionId) ?? false;

            if (!isValidAnswer)
            {
                return ResultSaveStatus.InvalidQuestionOrAnswer;
            }

            var existing = await _resultRepository.GetTrackedByRespondentAndQuestionAsync(respondent.Id, dto.SurveyQuestionId);
            if (existing is null)
            {
                await _resultRepository.AddAsync(new ResponseModel
                {
                    RespondentId = respondent.Id,
                    SurveyQuestionId = dto.SurveyQuestionId,
                    QuestionAnswerOptionId = dto.QuestionAnswerOptionId
                });
            }
            else
            {
                existing.QuestionAnswerOptionId = dto.QuestionAnswerOptionId;
                await _resultRepository.SaveChangesAsync();
            }

            return ResultSaveStatus.Success;
        }

        public async Task<ResultSaveStatus> CompleteAsync(string token)
        {
            var respondent = await _respondentRepository.GetTrackedByTokenAsync(token);
            if (respondent is null)
            {
                return ResultSaveStatus.TokenNotFound;
            }

            if (respondent.TokenIsUsed)
            {
                return ResultSaveStatus.AlreadyCompleted;
            }

            respondent.TokenIsUsed = true;
            respondent.TokenUsedAt = DateTime.UtcNow;
            await _respondentRepository.SaveChangesAsync();

            return ResultSaveStatus.Success;
        }
    }
}
