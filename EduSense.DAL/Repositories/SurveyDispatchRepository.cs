using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Repositories
{
    public class SurveyDispatchRepository : ISurveyDispatchRepository
    {
        //DI av EduSenseDbContext via konstruktor
        private readonly EduSenseDbContext _context;
        public SurveyDispatchRepository(EduSenseDbContext context) 
        {
            _context = context;        
        }

        // Metod för att hämta alla SurveyDispatches

        public async Task<IReadOnlyList<SurveyDispatchModel>> GetAllAsync()
        {
            return await _context.SurveyDispatches
                .AsNoTracking()
                .Include(d => d.Survey)
                .Include(d => d.Respondents)
                .ThenInclude(r => r.Responses)
                .OrderByDescending(d => d.SentAt)
                .ToListAsync();
        }

        // Metod för att hämta alla SurveyDispatches för en specifik surveyId
        public async Task<IReadOnlyList<SurveyDispatchModel>> GetAllForSurveyAsync(int surveyId)
        {
            return await _context.SurveyDispatches
               .AsNoTracking()
               .Where(d => d.SurveyId == surveyId)
               .Include(d => d.Survey)
               .Include(d => d.Respondents)
                   .ThenInclude(r => r.Responses)
               .OrderByDescending(d => d.SentAt)
               .ToListAsync();
        }

        // Metod för att hämta en SurveyDispatch med ett specifikt id
        public async Task<SurveyDispatchModel?> GetByIdAsync(int id)
        {
            return await _context.SurveyDispatches
                .AsNoTracking()
                .Include(d => d.Survey)
                .Include(d => d.Respondents)
                    .ThenInclude(r => r.Responses)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        // Metod för att hämta en SurveyDispatch med ett specifikt id, inklusive resultaten
        public async Task<SurveyDispatchModel?> GetByIdWithResultsAsync(int id)
        {
            return await _context.SurveyDispatches
                .Include(d => d.Survey)
                .Include(d => d.Respondents)
                    .ThenInclude(r => r.Responses)
                        .ThenInclude(resp => resp.SurveyQuestion!)
                            .ThenInclude(sq => sq.Question)
                .Include(d => d.Respondents)
                    .ThenInclude(r => r.Responses)
                        .ThenInclude(resp => resp.QuestionAnswerOption!)
                            .ThenInclude(qao => qao.AnswerOption)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        // Metod för att kontrollera om en SurveyDispatch med ett specifikt surveyId finns
        public async Task<bool> SurveyExistsAsync(int surveyId)
        {
            return await _context.Surveys.AnyAsync(sd => sd.Id == surveyId);
        }

        // Metod för att lägga till en ny SurveyDispatch
        public async Task AddAsync(SurveyDispatchModel dispatch)
        {
            _context.SurveyDispatches.Add(dispatch);
            await _context.SaveChangesAsync();
        }

        // Metod för att ta bort en SurveyDispatch med ett specifikt id
        public async Task<bool> DeleteAsync(int id)
        {
            var surveyDispatch = await _context.SurveyDispatches.FindAsync(id);
            if (surveyDispatch == null)
            {
                return false;
            }

            _context.SurveyDispatches.Remove(surveyDispatch);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
