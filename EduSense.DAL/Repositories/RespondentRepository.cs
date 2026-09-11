using System;
using System.Collections.Generic;
using System.Text;
using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Repositories
{
    public class RespondentRepository : IRespondentRepository
    {
        private readonly EduSenseDbContext _context;
        public RespondentRepository(EduSenseDbContext context)
        {
            _context = context;
        }

        public async Task<RespondentModel?> GetByTokenAsync(string token)
        {
            return await _context.Respondents
                .AsNoTracking()
                // ! (null-forgiving): Survey/Question är nullable i modellen, men
                // FK:erna är NOT NULL i databasen - de finns alltid i praktiken.
                .Include(r => r.Survey!)
                    .ThenInclude(s => s.SurveyQuestions)
                        .ThenInclude(sq => sq.Question!)
                            .ThenInclude(q => q.QuestionAnswerOptions)
                                .ThenInclude(qao => qao.AnswerOption)
                .Include(r => r.Responses)
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task<RespondentModel?> GetTrackedByTokenAsync(string token)
        {
            return await _context.Respondents
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
