using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Repositories
{
    public class ResultRepository : IResultRepository
    {
        private readonly EduSenseDbContext _context;

        public ResultRepository(EduSenseDbContext context)
        {
            _context = context;
        }

        private IQueryable<RespondentModel> RespondentsWithResponses()
        {
            return _context.Respondents
                .Include(r => r.Responses)
                    .ThenInclude(resp => resp.SurveyQuestion!)
                        .ThenInclude(sq => sq.Question!)
                            .ThenInclude(q => q.Category)
                .Include(r => r.Responses)
                    .ThenInclude(resp => resp.SurveyQuestion!)
                        .ThenInclude(sq => sq.Question!)
                            .ThenInclude(q => q.QuestionAnswerOptions)
                .Include(r => r.Responses)
                    .ThenInclude(resp => resp.QuestionAnswerOption!)
                        .ThenInclude(qao => qao.AnswerOption);
        }

        public async Task<IReadOnlyList<RespondentModel>> GetRespondentsForSurveyAsync(int surveyId)
        {
            return await RespondentsWithResponses()
                .Where(r => r.SurveyDispatch!.SurveyId == surveyId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<RespondentModel>> GetRespondentsForDispatchAsync(int dispatchId)
        {
            return await RespondentsWithResponses()
                .Where(r => r.SurveyDispatchId == dispatchId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<OrganisationModel>> GetOrganisationsWithResponsesAsync()
        {
            return await _context.Organisations
                .Include(o => o.Surveys)
                    .ThenInclude(s => s.Dispatches)
                        .ThenInclude(d => d.Respondents)
                            .ThenInclude(r => r.Responses)
                                .ThenInclude(resp => resp.QuestionAnswerOption!)
                                    .ThenInclude(qao => qao.AnswerOption)
                .ToListAsync();
        }

        public async Task<int?> GetPreviousDispatchIdAsync(int dispatchId)
        {
            var dispatch = await _context.SurveyDispatches.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dispatchId);
            if (dispatch is null)
            {
                return null;
            }

            return await _context.SurveyDispatches
                .Where(d => d.SurveyId == dispatch.SurveyId && d.SentAt < dispatch.SentAt)
                .OrderByDescending(d => d.SentAt)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<ResponseModel?> GetTrackedByRespondentAndQuestionAsync(int respondentId, int surveyQuestionId)
        {
            return await _context.Responses
                .FirstOrDefaultAsync(r => r.RespondentId == respondentId && r.SurveyQuestionId == surveyQuestionId);
        }

        public async Task AddAsync(ResponseModel response)
        {
            _context.Responses.Add(response);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
