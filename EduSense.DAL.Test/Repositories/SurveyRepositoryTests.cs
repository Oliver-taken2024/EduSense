using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduSense.DAL.Test.Repositories
{

    public class SurveyRepositoryTests
    {
        [Fact]
        public async Task AddAsync_persists_survey()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            var survey = new SurveyModel
            {
                Title = "Ny enkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id,
            };

            await repository.AddAsync(survey);

            Assert.True(survey.Id > 0);
            Assert.Equal(1, await context.Surveys.CountAsync());
        }

        [Fact]
        public async Task GetByIdAsync_returns_survey_with_organisation_and_question_text()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var question = new QuestionModel { Text = "Hur nöjd är du?", CreatedByUserId = "user-1" };
            context.Questions.Add(question);

            var survey = new SurveyModel
            {
                Title = "Kundenkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            context.SurveyQuestions.Add(new SurveyQuestionModel { SurveyId = survey.Id, QuestionId = question.Id });
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            var result = await repository.GetByIdAsync(survey.Id);

            Assert.NotNull(result);
            Assert.Equal("Business AB", result!.Organisation!.Name);
            var surveyQuestion = Assert.Single(result.SurveyQuestions);
            Assert.Equal("Hur nöjd är nu?", surveyQuestion.Question!.Text);
        }


        [Fact]
        public async Task GetByIdAsync_returns_null_if_survey_not_exists()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var repository = new SurveyRepository(context);
            var result = await repository.GetByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTrackedByIdAsync_returns_tracked_entity()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var survey = new SurveyModel
            {
            Title = "Gammal titel",
            CreatedByUserId = "user-1",
            SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
            OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            var tracked = await repository.GetTrackedByIdAsync(survey.Id);

            Assert.NotNull(tracked);
            tracked!.Title = "Ny titel";
            await context.SaveChangesAsync();

            var reloaded = await context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == survey.Id);
            Assert.Equal("Ny titel", reloaded?.Title);
        }
        [Fact]
        public async Task DeleteAsync_removes_properly_and_returns_true()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var survey = new SurveyModel
            {
                Title = "Gammal titel",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = 1
            };

            var repository = new SurveyRepository(context);
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var result = await repository.DeleteAsync(survey.Id);

            Assert.True(result);
            Assert.Null(repository.GetByIdAsync(survey.Id));
            Assert.Empty(context.Surveys);
        }

        [Fact]
        public async Task DeleteAsync_returns_false_if_survey_not_exists ()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var survey = new SurveyModel
            {
                Title = "Gammal titel",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = 1
            };

            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);

            var result = await repository.TitleExistsAsync("Ny titel", survey.SurveyExpiryDate, 1, null);

            Assert.False(result);

        }


    }
}
