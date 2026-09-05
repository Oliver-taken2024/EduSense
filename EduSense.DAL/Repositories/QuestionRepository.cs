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

        public async Task<IReadOnlyList<QuestionWithOrganisationModel>> GetAllWithOrganisationAsync()
        {
            var query =
                from question in _context.Questions.AsNoTracking()
                join orgUser in _context.OrganisationUsers.AsNoTracking()
                    on question.CreatedByUserId equals orgUser.UserId into orgUsers
                from orgUser in orgUsers.DefaultIfEmpty()
                join organisation in _context.Organisations.AsNoTracking()
                    on orgUser.OrganisationId equals organisation.Id into organisations
                from organisation in organisations.DefaultIfEmpty()
                select new QuestionWithOrganisationModel
                {
                    Question = question,
                    Organisation = organisation
                };

            return await query.ToListAsync();
        }

        public async Task<QuestionWithOrganisationModel?> GetByIdWithOrganisationAsync(int id)
        {
            var query =
                from question in _context.Questions.AsNoTracking()
                where question.Id == id
                join orgUser in _context.OrganisationUsers.AsNoTracking()
                    on question.CreatedByUserId equals orgUser.UserId into orgUsers
                from orgUser in orgUsers.DefaultIfEmpty()
                join organisation in _context.Organisations.AsNoTracking()
                    on orgUser.OrganisationId equals organisation.Id into organisations
                from organisation in organisations.DefaultIfEmpty()
                select new QuestionWithOrganisationModel
                {
                    Question = question,
                    Organisation = organisation
                };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<QuestionModel?> GetByIdAsync(int id)
        {
            return await _context.Questions.FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<QuestionModel> CreateAsync(QuestionModel question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<QuestionModel> UpdateAsync(QuestionModel question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task DeleteAsync(QuestionModel question)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
           
        }   
    }
}