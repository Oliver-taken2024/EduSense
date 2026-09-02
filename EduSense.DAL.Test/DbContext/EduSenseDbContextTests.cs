using EduSense.DAL.Data;
using EduSense.DAL.Models;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Test.DbContexts;

public class EduSenseDbContextTests
{
    [Fact]
    public async Task Can_save_organisation_and_survey()
    {
        //Skapa app-context mot sqlite db
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        //Skapa en OrganisationModel och spara den
        var organisation = new OrganisationModel
        {
            Name = "Org 1"
        };

        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        //Skapa en survey och spara den
        var survey = new SurveyModel
        {
            Title = "Survey 1",
            CreatedByUserId = "user-1",
            SurveyExpiryDate = DateTime.UtcNow.AddDays(7),
            OrganisationId = organisation.Id
        };

        context.Surveys.Add(survey);
        await context.SaveChangesAsync();

        //Läser tillbaka och kollar att det bara finns en enda survey i db
        var savedSurvey = await context.Surveys.SingleAsync();

        //Kolla att den läses tillbaka korrekt.
        Assert.Equal("Survey 1", savedSurvey.Title);
        Assert.Equal(organisation.Id, savedSurvey.OrganisationId);
    }

    [Fact]
    public async Task Duplicate_survey_question_is_rejected()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        //Skapa organisation, survey och fråga och spara ner
        var organisation = new OrganisationModel
        {
            Name = "Org 1"
        };
        context.Organisations.Add(organisation);

        var survey = new SurveyModel
        {
            Title = "Survey 1",
            CreatedByUserId = "user-1",
            SurveyExpiryDate = DateTime.UtcNow.AddDays(7),
            Organisation = organisation
        };
        context.Surveys.Add(survey);

        var question = new QuestionModel
        {
            Text = "Question 1",
            CreatedByUserId = "user-1"
        };
        context.Questions.Add(question);

        await context.SaveChangesAsync();

        //Testa att spara samma fråga till samma survey och se att det inte går
        context.SurveyQuestions.Add(new SurveyQuestionModel
        {
            SurveyId = survey.Id,
            QuestionId = question.Id
        });
        await context.SaveChangesAsync();

        context.SurveyQuestions.Add(new SurveyQuestionModel
        {
            SurveyId = survey.Id,
            QuestionId = question.Id
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
