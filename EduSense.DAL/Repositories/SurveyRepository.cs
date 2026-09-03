using System;
using System.Collections.Generic;
using System.Text;

using EduSense.DAL.Models;
using EduSense.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Repositories
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly EduSenseDbContext _context;

        public SurveyRepository(EduSenseDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<SurveyModel>> GetAllAsync()
        {
            return await _context.Surveys
                .AsNoTracking()
                .Include(s => s.Organisation)
                .Include(s => s.SurveyQuestions)
                    .ThenInclude(sq => sq.Question)
                .ToListAsync();
        }

        public async Task<SurveyModel?> GetByIdAsync(int id)
        {
            return await _context.Surveys
                .AsNoTracking()
                .Include(s => s.Organisation)
                .Include(s => s.SurveyQuestions)
                    .ThenInclude(sq => sq.Question)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SurveyModel?> GetTrackedByIdAsync(int id)
        {
            return await _context.Surveys
                .Include(s => s.SurveyQuestions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(SurveyModel survey)
        {
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            if (survey is null)
            {
                return false;
            }

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<int>> GetExistingQuestionIdsAsync(IEnumerable<int> questionIds)
        {
            return await _context.Questions
                .Where(q => questionIds.Contains(q.Id))
                .Select(q => q.Id)
                .ToListAsync();
        }

        public async Task<bool> TitleExistsAsync(string title, DateTime surveyExpiryDate, int organisationId, int? excludeSurveyId = null)
        {
            return await _context.Surveys
                .Where(s => s.Title == title
                    && s.SurveyExpiryDate == surveyExpiryDate
                    && s.OrganisationId == organisationId
                    && (excludeSurveyId == null || s.Id != excludeSurveyId))
                .AnyAsync();
        }

    }
}
