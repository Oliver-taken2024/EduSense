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

            var dispatch = respondent.SurveyDispatch;
            var survey = dispatch?.Survey;
            if (survey is null)
            {
                throw new InvalidOperationException("Respondent saknar Survey.");
            }

            if (dispatch!.ResponseDeadline < DateTime.UtcNow)
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

        public async Task<SurveyResultDto> GetResultForSurveyAsync(int surveyId)
        {
            var respondents = await _resultRepository.GetRespondentsForSurveyAsync(surveyId);
            return BuildResult(respondents);
        }

        public async Task<SurveyResultDto> GetResultForDispatchAsync(int dispatchId)
        {
            var respondents = await _resultRepository.GetRespondentsForDispatchAsync(dispatchId);
            var result = BuildResult(respondents);

            // "Föregående period" = föregående utskick av samma enkät, kronologiskt.
            var previousDispatchId = await _resultRepository.GetPreviousDispatchIdAsync(dispatchId);
            if (previousDispatchId.HasValue)
            {
                var previousRespondents = await _resultRepository.GetRespondentsForDispatchAsync(previousDispatchId.Value);
                var previousResult = BuildResult(previousRespondents);
                ApplyPeriodComparison(result, previousResult);
            }

            return result;
        }

        private static void ApplyPeriodComparison(SurveyResultDto current, SurveyResultDto previous)
        {
            current.TotalResponsesChangePercent = previous.TotalResponses == 0
                ? null
                : (double)(current.TotalResponses - previous.TotalResponses) / previous.TotalResponses * 100;

            current.ResponseRateChangePoints = current.ResponseRate - previous.ResponseRate;
            current.AverageScoreChangePoints = current.AverageScore - previous.AverageScore;
            current.NpsChangePoints = current.Nps.HasValue && previous.Nps.HasValue
                ? current.Nps.Value - previous.Nps.Value
                : null;
            current.CriticalAreasCountChange = current.CriticalAreasCount - previous.CriticalAreasCount;
        }

        private static SurveyResultDto BuildResult(IReadOnlyList<RespondentModel> respondents)
        {
            var completed = respondents.Where(r => r.TokenIsUsed).ToList();

            var allAnswers = completed
                .SelectMany(r => r.Responses.Select(resp => new
                {
                    RespondentId = r.Id,
                    Segment = r.Segment,
                    TokenUsedAt = r.TokenUsedAt,
                    Question = resp.SurveyQuestion!.Question!,
                    Value = resp.QuestionAnswerOption!.AnswerOption!.Value,
                    Description = resp.QuestionAnswerOption!.AnswerOption!.Description
                }))
                .ToList();

            // NPS-frågor har en egen 1-10-skala (10 svarsalternativ) istället för
            // den vanliga 1-5-skalan - avgörs av frågans uppsättning, inte av inkomna svar.
            var npsQuestionIds = allAnswers
                .Where(a => a.Question.QuestionAnswerOptions.Count == 10)
                .Select(a => a.Question.Id)
                .ToHashSet();

            var regularAnswers = allAnswers.Where(a => !npsQuestionIds.Contains(a.Question.Id)).ToList();
            var npsAnswers = allAnswers.Where(a => npsQuestionIds.Contains(a.Question.Id)).ToList();
            var npsValues = npsAnswers.Select(a => a.Value).ToList();

            var valuesByRespondent = regularAnswers
                .GroupBy(a => a.RespondentId)
                .ToDictionary(g => g.Key, g => g.Select(a => a.Value).ToList());

            // Snitt per fråga - grund för både topplistor och antal kritiska områden.
            var questionScores = regularAnswers
                .GroupBy(a => a.Question)
                .Select(g => new QuestionScoreDto { QuestionText = g.Key.Text, AverageScore = g.Average(a => a.Value) })
                .ToList();

            return new SurveyResultDto
            {
                TotalResponses = completed.Count,
                ResponseRate = respondents.Count == 0 ? 0 : (double)completed.Count / respondents.Count * 100,
                AverageScore = regularAnswers.Count == 0 ? 0 : regularAnswers.Average(a => a.Value),
                Nps = npsValues.Count == 0 ? null : CalculateNps(npsValues),
                // Under 3.0 på en 1-5-skala räknas som ett kritiskt område.
                CriticalAreasCount = questionScores.Count(q => q.AverageScore < 3.0),
                CategoryScores = regularAnswers
                    .Where(a => a.Question.Category is not null)
                    .GroupBy(a => a.Question.Category!.Name)
                    .Select(g => new CategoryScoreDto { CategoryName = g.Key, AverageScore = g.Average(a => a.Value) })
                    .ToList(),
                AnswerDistribution = regularAnswers
                    .GroupBy(a => a.Description)
                    .Select(g => new AnswerDistributionDto
                    {
                        AnswerDescription = g.Key,
                        Count = g.Count(),
                        Percentage = regularAnswers.Count == 0 ? 0 : (double)g.Count() / regularAnswers.Count * 100
                    })
                    .ToList(),
                SegmentResults = respondents
                    .GroupBy(r => r.Segment)
                    .Select(g =>
                    {
                        var values = g.SelectMany(r => valuesByRespondent.TryGetValue(r.Id, out var v) ? v : []).ToList();
                        var segmentNpsValues = npsAnswers.Where(a => a.Segment == g.Key).Select(a => a.Value).ToList();
                        return new SegmentResultDto
                        {
                            Segment = MapSegmentToDto(g.Key),
                            RespondentCount = g.Count(),
                            ResponseRate = (double)g.Count(r => r.TokenIsUsed) / g.Count() * 100,
                            AverageScore = values.Count == 0 ? 0 : values.Average(),
                            Nps = segmentNpsValues.Count == 0 ? null : CalculateNps(segmentNpsValues)
                        };
                    })
                    .ToList(),
                TopStrengths = questionScores.OrderByDescending(q => q.AverageScore).Take(3).ToList(),
                Challenges = questionScores.OrderBy(q => q.AverageScore).Take(3).ToList(),
                // Grupperat per månad utifrån när respondenten faktiskt svarade (TokenUsedAt).
                Trend = regularAnswers
                    .Where(a => a.Question.Category is not null && a.TokenUsedAt.HasValue)
                    .GroupBy(a => new
                    {
                        Period = new DateTime(a.TokenUsedAt!.Value.Year, a.TokenUsedAt.Value.Month, 1),
                        CategoryName = a.Question.Category!.Name
                    })
                    .Select(g => new TrendPointDto
                    {
                        PeriodStart = g.Key.Period,
                        CategoryName = g.Key.CategoryName,
                        AverageScore = g.Average(a => a.Value)
                    })
                    .OrderBy(t => t.PeriodStart)
                    .ThenBy(t => t.CategoryName)
                    .ToList()
            };
        }

        public async Task<IReadOnlyList<OrganisationLocationDto>> GetOrganisationOverviewAsync()
        {
            var organisations = await _resultRepository.GetOrganisationsWithResponsesAsync();

            return organisations.Select(o =>
            {
                var respondentsList = o.Surveys
                    .SelectMany(s => s.Dispatches)
                    .SelectMany(d => d.Respondents)
                    .ToList();

                var values = respondentsList
                    .Where(r => r.TokenIsUsed)
                    .SelectMany(r => r.Responses)
                    .Where(resp => resp.QuestionAnswerOption?.AnswerOption is not null)
                    .Select(resp => resp.QuestionAnswerOption!.AnswerOption!.Value)
                    .Where(v => v <= 5) // exkludera NPS-svar (1-10-skalan) ur skol-snittet
                    .ToList();

                return new OrganisationLocationDto
                {
                    OrganisationName = o.Name,
                    Latitude = o.Latitude,
                    Longitude = o.Longitude,
                    AverageScore = values.Count == 0 ? 0 : values.Average(),
                    RespondentCount = respondentsList.Count(r => r.TokenIsUsed)
                };
            }).ToList();
        }

        private static double CalculateNps(IReadOnlyList<int> npsValues)
        {
            var promoters = npsValues.Count(v => v >= 9);
            var detractors = npsValues.Count(v => v <= 6);
            return (double)(promoters - detractors) / npsValues.Count * 100;
        }

        private static RespondentSegmentDto MapSegmentToDto(RespondentSegment segment) => segment switch
        {
            RespondentSegment.GradeFTo6 => RespondentSegmentDto.GradeFTo6,
            RespondentSegment.Grade7To9 => RespondentSegmentDto.Grade7To9,
            RespondentSegment.Gymnasiet => RespondentSegmentDto.Gymnasiet,
            RespondentSegment.Vuxenutbildning => RespondentSegmentDto.Vuxenutbildning,
            RespondentSegment.Personal => RespondentSegmentDto.Personal,
            _ => throw new ArgumentOutOfRangeException(nameof(segment))
        };
    }
}
