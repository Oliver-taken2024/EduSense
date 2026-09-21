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
                from question in _context.Questions.AsNoTracking().Include(q => q.Category)
                join orgUser in _context.OrganisationUsers.AsNoTracking()
                    on question.CreatedByUserId equals orgUser.UserId into orgUsers
                from orgUser in orgUsers.DefaultIfEmpty()
                join organisation in _context.Organisations.AsNoTracking()
                    on orgUser.OrganisationId equals organisation.Id into organisations
                from organisation in organisations.DefaultIfEmpty()
                // Nyast skapade fråga ska hamna överst i listan, inte längst bak.
                // Id som sekundär sortering - flera fröade frågor delar samma CreatedAt-tidsstämpel.
                orderby question.CreatedAt descending, question.Id descending
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
                from question in _context.Questions.AsNoTracking().Include(q => q.Category)
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

        // Samma fasta 1-5-skala som DataSeeder länkar de fröade frågorna mot. Utan denna
        // koppling saknar en fråga skapad via admin-UI:t svarsalternativ helt, och
        // respondenter kan inte välja något på frågesidan.
        private static readonly (string Description, int Value)[] StandardAnswerScale =
        [
            ("Mycket missnöjd", 1),
            ("Missnöjd", 2),
            ("Neutral", 3),
            ("Nöjd", 4),
            ("Mycket nöjd", 5)
        ];

        public async Task<QuestionModel> CreateAsync(QuestionModel question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            foreach (var (description, value) in StandardAnswerScale)
            {
                var option = await _context.AnswerOptions
                    .SingleOrDefaultAsync(x => x.Description == description && x.Value == value);

                if (option is null)
                {
                    option = new AnswerOptionModel { Description = description, Value = value };
                    _context.AnswerOptions.Add(option);
                    await _context.SaveChangesAsync();
                }

                _context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel
                {
                    QuestionId = question.Id,
                    AnswerOptionId = option.Id
                });
            }

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