using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduSense.DAL.Test.Repositories
{
    public class RespondentRepositoryTests
    {
        [Fact]
        public async Task GetByTokenAsync_UnknownToken_ReturnsNull()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var repository = new RespondentRepository(scope.Context);

            var result = await repository.GetByTokenAsync("finns-inte");

            Assert.Null(result);
        }
    

        [Fact]
        public async Task GetByTokenAsync_WithData_ReturnsFullIncludeChain()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var question = new QuestionModel { Text = "Trivs du?", CreatedByUserId = "user-1" };
            var answerOption = new AnswerOptionModel { Description = "Ja", Value = 1 };
            context.Questions.Add(question);
            context.AnswerOptions.Add(answerOption);

            var survey = new SurveyModel
            {
                Title = "Enkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var questionAnswerOption = new QuestionAnswerOptionModel { QuestionId = question.Id, AnswerOptionId = answerOption.Id };
            context.QuestionAnswerOptions.Add(questionAnswerOption);

            var surveyQuestion = new SurveyQuestionModel { SurveyId = survey.Id, QuestionId = question.Id };
            context.SurveyQuestions.Add(surveyQuestion);
            await context.SaveChangesAsync();

            var respondent = new RespondentModel { Email = "test@test.se", Token = "token-1", SurveyId = survey.Id };
            context.Respondents.Add(respondent);
            await context.SaveChangesAsync();

            context.Responses.Add(new ResponseModel
            {
                RespondentId = respondent.Id,
                SurveyQuestionId = surveyQuestion.Id,
                QuestionAnswerOptionId = questionAnswerOption.Id
            });
            await context.SaveChangesAsync();

            var repository = new RespondentRepository(context);
            var result = await repository.GetByTokenAsync("token-1");

            Assert.NotNull(result);
            Assert.Equal("Enkät", result!.Survey!.Title);

            var loadedSurveyQuestion = Assert.Single(result.Survey.SurveyQuestions);
            Assert.Equal("Trivs du?", loadedSurveyQuestion.Question!.Text);

            var loadedQao = Assert.Single(loadedSurveyQuestion.Question.QuestionAnswerOptions);
            Assert.Equal("Ja", loadedQao.AnswerOption!.Description);

            Assert.Single(result.Responses);
        }

        [Fact]
        public async Task GetTrackedByTokenAsync_ExistingToken_ReturnsTrackedEntity()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var survey = new SurveyModel
            {
                Title = "Enkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            context.Respondents.Add(new RespondentModel { Email = "test@test.se", Token = "token-1", SurveyId = survey.Id });
            await context.SaveChangesAsync();

            var repository = new RespondentRepository(context);
            var result = await repository.GetTrackedByTokenAsync("token-1");

            Assert.NotNull(result);
            Assert.Equal(EntityState.Unchanged, context.Entry(result!).State);
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsChanges()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var survey = new SurveyModel
            {
                Title = "Enkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            context.Respondents.Add(new RespondentModel { Email = "test@test.se", Token = "token-1", SurveyId = survey.Id });
            await context.SaveChangesAsync();

            var repository = new RespondentRepository(context);
            var tracked = await repository.GetTrackedByTokenAsync("token-1");
            tracked!.TokenIsUsed = true;
            await repository.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var reloaded = await context.Respondents.SingleAsync(r => r.Token == "token-1");
            Assert.True(reloaded.TokenIsUsed);
        }

    } 
}
