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
            Assert.Equal("Hur nöjd är du?", surveyQuestion.Question!.Text);
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

            var repository = new SurveyRepository(context);
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var result = await repository.DeleteAsync(survey.Id);

            Assert.True(result);
            Assert.Null(await repository.GetByIdAsync(survey.Id));
            Assert.Empty(context.Surveys);
        }

        [Fact]
        public async Task TitleExistsAsync_returns_false_for_different_title()
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

            var result = await repository.TitleExistsAsync("Ny titel", survey.SurveyExpiryDate, organisation.Id, null);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_should_delete_related_surveyquestions()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);

            var question = new QuestionModel { Text = "Fråga 1", CreatedByUserId = "user-1" };
            context.Questions.Add(question);
            await context.SaveChangesAsync();

            var survey = new SurveyModel
            {
                Title = "Enkät med frågor",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            context.SurveyQuestions.Add(new SurveyQuestionModel { SurveyId = survey.Id, QuestionId = question.Id });
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            await repository.DeleteAsync(survey.Id);

            Assert.Empty(context.SurveyQuestions);
            Assert.Equal(1, await context.Questions.CountAsync());
        }

        [Fact]
        public async Task GetExistingQuestionIdsAsync_returns_only_ids_that_exist()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var question = new QuestionModel { Text = "Fråga 1", CreatedByUserId = "user-1" };
            context.Questions.Add(question);
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            var result = await repository.GetExistingQuestionIdsAsync(new[] { question.Id, 999 });

            var existingId = Assert.Single(result);
            Assert.Equal(question.Id, existingId);
        }

        [Fact]
        public async Task TitleExistsAsync_returns_true_for_matching_combination()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var survey = new SurveyModel
            {
                Title = "Kundnöjdhet",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            var result = await repository.TitleExistsAsync("Kundnöjdhet", survey.SurveyExpiryDate, organisation.Id, null);

            Assert.True(result);
        }

        [Fact]
        public async Task TitleExistsAsync_excludes_given_survey_id()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var survey = new SurveyModel
            {
                Title = "Kundnöjdhet",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var repository = new SurveyRepository(context);
            var result = await repository.TitleExistsAsync(survey.Title, survey.SurveyExpiryDate, organisation.Id, survey.Id);

            Assert.False(result);
        }
    }
}
