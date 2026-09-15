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

        [Fact]
        public async Task GetRespondentsForSurveyAsync_ReturnsRespondentsWithFullResponseGraph()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;
            var (_, survey, _, _, respondent) = await SeedFullScenarioAsync(context);

            var repository = new ResultRepository(context);
            var result = await repository.GetRespondentsForSurveyAsync(survey.Id);

            var loaded = Assert.Single(result);
            Assert.Equal(respondent.Id, loaded.Id);
            var response = Assert.Single(loaded.Responses);
            Assert.Equal("Trivsel", response.SurveyQuestion!.Question!.Category!.Name);
            Assert.Equal(5, response.QuestionAnswerOption!.AnswerOption!.Value);
        }

        [Fact]
        public async Task GetRespondentsForDispatchAsync_OnlyReturnsRespondentsForThatDispatch()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;
            await SeedFullScenarioAsync(context, sentAt: DateTime.UtcNow.AddMonths(-1));
            var (_, _, dispatch2, _, respondent2) = await SeedFullScenarioAsync(context, sentAt: DateTime.UtcNow);

            var repository = new ResultRepository(context);
            var result = await repository.GetRespondentsForDispatchAsync(dispatch2.Id);

            var loaded = Assert.Single(result);
            Assert.Equal(respondent2.Id, loaded.Id);
        }

        [Fact]
        public async Task GetOrganisationsWithResponsesAsync_ReturnsNestedResponseGraph()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;
            var (organisation, _, _, _, respondent) = await SeedFullScenarioAsync(context);

            var repository = new ResultRepository(context);
            var result = await repository.GetOrganisationsWithResponsesAsync();

            var loadedOrg = Assert.Single(result);
            Assert.Equal(organisation.Id, loadedOrg.Id);
            var loadedRespondent = Assert.Single(loadedOrg.Surveys.SelectMany(s => s.Dispatches).SelectMany(d => d.Respondents));
            Assert.Equal(respondent.Id, loadedRespondent.Id);
            var response = Assert.Single(loadedRespondent.Responses);
            Assert.Equal(5, response.QuestionAnswerOption!.AnswerOption!.Value);
        }

        [Fact]
        public async Task GetPreviousDispatchIdAsync_ReturnsEarlierDispatchForSameSurvey_AndNullWhenNoneExists()
        {
            using var scope = TestDbContextFactory.CreateAppContext();
            var context = scope.Context;

            var organisation = new OrganisationModel { Name = "Skola A" };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var survey = new SurveyModel { Title = "Enkät", CreatedByUserId = "user-1", OrganisationId = organisation.Id };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var earlierDispatch = new SurveyDispatchModel
            {
                SurveyId = survey.Id,
                ResponseDeadline = DateTime.UtcNow.AddDays(10),
                SentByUserId = "user-1",
                SentAt = DateTime.UtcNow.AddMonths(-2)
            };
            var laterDispatch = new SurveyDispatchModel
            {
                SurveyId = survey.Id,
                ResponseDeadline = DateTime.UtcNow.AddDays(40),
                SentByUserId = "user-1",
                SentAt = DateTime.UtcNow
            };
            context.SurveyDispatches.AddRange(earlierDispatch, laterDispatch);
            await context.SaveChangesAsync();

            var repository = new ResultRepository(context);

            var previousForLater = await repository.GetPreviousDispatchIdAsync(laterDispatch.Id);
            var previousForEarlier = await repository.GetPreviousDispatchIdAsync(earlierDispatch.Id);

            Assert.Equal(earlierDispatch.Id, previousForLater);
            Assert.Null(previousForEarlier);
        }

        // Seedar organisation -> kategori -> fråga -> enkät -> utskick -> respondent (+ svar om completed),
        // med all data som GetRespondentsFor*/GetOrganisationsWithResponsesAsync faktiskt behöver läsa in.
        private static async Task<(OrganisationModel Organisation, SurveyModel Survey, SurveyDispatchModel Dispatch, SurveyQuestionModel SurveyQuestion, RespondentModel Respondent)> SeedFullScenarioAsync(
            EduSenseDbContext context, DateTime? sentAt = null, bool completed = true)
        {
            var organisation = new OrganisationModel { Name = "Skola A", Latitude = 55.6, Longitude = 13.0 };
            context.Organisations.Add(organisation);
            await context.SaveChangesAsync();

            var category = new CategoryModel { Name = "Trivsel" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var question = new QuestionModel { Text = "Trivs du?", CreatedByUserId = "user-1", CategoryId = category.Id };
            context.Questions.Add(question);

            var answerOption = new AnswerOptionModel { Description = "Ja", Value = 5 };
            context.AnswerOptions.Add(answerOption);

            var survey = new SurveyModel { Title = "Enkät", CreatedByUserId = "user-1", OrganisationId = organisation.Id };
            context.Surveys.Add(survey);
            await context.SaveChangesAsync();

            var surveyQuestion = new SurveyQuestionModel { SurveyId = survey.Id, QuestionId = question.Id };
            context.SurveyQuestions.Add(surveyQuestion);

            var questionAnswerOption = new QuestionAnswerOptionModel { QuestionId = question.Id, AnswerOptionId = answerOption.Id };
            context.QuestionAnswerOptions.Add(questionAnswerOption);
            await context.SaveChangesAsync();

            var dispatch = new SurveyDispatchModel
            {
                SurveyId = survey.Id,
                ResponseDeadline = DateTime.UtcNow.AddDays(30),
                SentByUserId = "user-1",
                SentAt = sentAt ?? DateTime.UtcNow
            };
            context.SurveyDispatches.Add(dispatch);
            await context.SaveChangesAsync();

            var respondent = new RespondentModel
            {
                Email = "test@test.se",
                Token = Guid.NewGuid().ToString("N"),
                SurveyDispatchId = dispatch.Id,
                Segment = RespondentSegment.Personal,
                TokenIsUsed = completed,
                TokenUsedAt = completed ? DateTime.UtcNow : null
            };
            context.Respondents.Add(respondent);
            await context.SaveChangesAsync();

            if (completed)
            {
                context.Responses.Add(new ResponseModel
                {
                    RespondentId = respondent.Id,
                    SurveyQuestionId = surveyQuestion.Id,
                    QuestionAnswerOptionId = questionAnswerOption.Id
                });
                await context.SaveChangesAsync();
            }

            return (organisation, survey, dispatch, surveyQuestion, respondent);
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

            var dispatch = new SurveyDispatchModel { SurveyId = survey.Id, ResponseDeadline = DateTime.UtcNow, SentByUserId = "user-1" };
            context.SurveyDispatches.Add(dispatch);
            await context.SaveChangesAsync();

            var respondent = new RespondentModel { Email = "test@test.se", Token = "token-1", SurveyDispatchId = dispatch.Id };
            context.Respondents.Add(respondent);
            await context.SaveChangesAsync();

            return (respondent, surveyQuestion, questionAnswerOptionJa.Id, questionAnswerOptionNej.Id);
        }
    }
}
