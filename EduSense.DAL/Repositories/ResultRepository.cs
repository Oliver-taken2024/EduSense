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
