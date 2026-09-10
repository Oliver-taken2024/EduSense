using EduSense.DAL.Data;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduSense.DAL.Test.Repositories
{
    public class ResultRepositoryTests
    {
        [Fact]
        public async Task GetTrackedByRespondentAndQuestionAsync_NoExisting_ReturnsNull()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var repository = new ResultRepository(scope.Context);

            var result = await repository.GetTrackedByRespondentAndQuestionAsync(1, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTrackedByRespondentAndQuestionAsync_Existing_ReturnsTrackedEntity()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var (respondent, surveyQuestion, answerOptionId1, _) = await SeedRespondentWithQuestionAsync(context);
            context.Responses.Add(new ResponseModel
            {
                RespondentId = respondent.Id,
                SurveyQuestionId = surveyQuestion.Id,
                QuestionAnswerOptionId = answerOptionId1
            });
            await context.SaveChangesAsync();

            var repository = new ResultRepository(context);
            var result = await repository.GetTrackedByRespondentAndQuestionAsync(respondent.Id, surveyQuestion.Id);

            Assert.NotNull(result);
            Assert.Equal(EntityState.Unchanged, context.Entry(result!).State);
        }

        [Fact]
        public async Task AddAsync_PersistsResponse()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var (respondent, surveyQuestion, answerOptionId1, _) = await SeedRespondentWithQuestionAsync(context);

            var repository = new ResultRepository(context);
            await repository.AddAsync(new ResponseModel
            {
                RespondentId = respondent.Id,
                SurveyQuestionId = surveyQuestion.Id,
                QuestionAnswerOptionId = answerOptionId1
            });

            Assert.Equal(1, await context.Responses.CountAsync());
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsUpdatedAnswer()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var (respondent, surveyQuestion, answerOptionId1, answerOptionId2) = await SeedRespondentWithQuestionAsync(context);
            var response = new ResponseModel
            {
                RespondentId = respondent.Id,
                SurveyQuestionId = surveyQuestion.Id,
                QuestionAnswerOptionId = answerOptionId1
            };
            context.Responses.Add(response);
            await context.SaveChangesAsync();

            var repository = new ResultRepository(context);
            response.QuestionAnswerOptionId = answerOptionId2;
            await repository.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var reloaded = await context.Responses.SingleAsync(r => r.Id == response.Id);
            Assert.Equal(answerOptionId2, reloaded.QuestionAnswerOptionId);
        }

        private static async Task<(RespondentModel Respondent, SurveyQuestionModel SurveyQuestion, int AnswerOptionId1, int AnswerOptionId2)> SeedRespondentWithQuestionAsync(EduSenseDbContext context)
        {
            var organisation = new OrganisationModel { Name = "Business AB" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var question = new QuestionModel { Text = "Trivs du?", CreatedByUserId = "user-1" };
            context.Questions.Add(question);

            var answerOptionJa = new AnswerOptionModel { Description = "Ja", Value = 1 };
            var answerOptionNej = new AnswerOptionModel { Description = "Nej", Value = 0 };
            context.AnswerOptions.AddRange(answerOptionJa, answerOptionNej);

            var survey = new SurveyModel
            {
                Title = "Enkät",
                CreatedByUserId = "user-1",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = organisation.Id
            };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var surveyQuestion = new SurveyQuestionModel { SurveyId = survey.Id, QuestionId = question.Id };
            context.SurveyQuestions.Add(surveyQuestion);

            var questionAnswerOptionJa = new QuestionAnswerOptionModel { QuestionId = question.Id, AnswerOptionId = answerOptionJa.Id };
            var questionAnswerOptionNej = new QuestionAnswerOptionModel { QuestionId = question.Id, AnswerOptionId = answerOptionNej.Id };
            context.QuestionAnswerOptions.AddRange(questionAnswerOptionJa, questionAnswerOptionNej);
            await context.SaveChangesAsync();

            var respondent = new RespondentModel { Email = "test@test.se", Token = "token-1", SurveyId = survey.Id };
            context.Respondents.Add(respondent);
            await context.SaveChangesAsync();

            return (respondent, surveyQuestion, questionAnswerOptionJa.Id, questionAnswerOptionNej.Id);
        }
    }
}
