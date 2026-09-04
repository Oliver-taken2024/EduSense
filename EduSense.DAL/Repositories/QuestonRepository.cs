using System.Collections.Generic;

using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly EduSenseDbContext _context;

        public QuestionRepository(EduSenseDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<QuestionModel>> GetAllAsync()
        {
            return await _context.Questions
                .AsNoTracking()
                .ToListAsync();
        }
    }
}