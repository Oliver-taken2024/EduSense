using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduSense.DAL.Test.Repositories;

public class QuestionRepositoryTests
{
    [Fact]
    public async Task GetAllWithOrganisationAsync_returns_organisation_when_user_belongs_to_one()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        var organisation = new OrganisationModel { Name = "Business AB" };
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        context.OrganisationUsers.Add(new OrganisationUserModel
        {
            OrganisationId = organisation.Id,
            UserId = "user-1"
        });

        context.Questions.Add(new QuestionModel
        {
            Text = "Fråga 1",
            CreatedByUserId = "user-1"
        });

        await context.SaveChangesAsync();

        var repository = new QuestionRepository(context);
        var result = await repository.GetAllWithOrganisationAsync();

        var question = Assert.Single(result);
        Assert.Equal("Business AB", question.Organisation?.Name);
    }

    [Fact]
    public async Task GetAllWithOrganisationAsync_returns_null_organisation_when_user_has_no_organisation()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        context.Questions.Add(new QuestionModel
        {
            Text = "Fråga utan organisation",
            CreatedByUserId = "orphan-user"
        });
        await context.SaveChangesAsync();

        var repository = new QuestionRepository(context);
        var result = await repository.GetAllWithOrganisationAsync();

        var question = Assert.Single(result);
        Assert.Null(question.Organisation);
    }

    [Fact]
    public async Task CreateAsync_persists_question()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        var repository = new QuestionRepository(context);

        var created = await repository.CreateAsync(new QuestionModel
        {
            Text = "Ny fråga",
            CreatedByUserId = "user-1"
        });

        Assert.True(created.Id > 0);
        Assert.Equal(1, await context.Questions.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_removes_question()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        var question = new QuestionModel { Text = "Ta bort mig", CreatedByUserId = "user-1" };
        context.Questions.Add(question);
        await context.SaveChangesAsync();

        var repository = new QuestionRepository(context);
        await repository.DeleteAsync(question);

        Assert.Empty(context.Questions);
    }

    [Fact]
    public async Task UpdateAsync_modifies_question()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        var question = new QuestionModel { Text = "Uppdatera mig", CreatedByUserId = "user-1" };
        context.Questions.Add(question);

        await context.SaveChangesAsync();
        var repository = new QuestionRepository(context);
        question.Text = "Jag är uppdaterad";

        await repository.UpdateAsync(question);
        var updated = await context.Questions.FirstOrDefaultAsync(q => q.Id == question.Id);
        Assert.Equal("Jag är uppdaterad", updated?.Text);
    }

    [Fact]
    public async Task GetByIdAsync_returns_question_when_exists()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        var question = new QuestionModel { Text = "Hitta mig", CreatedByUserId = "user-1" };
        context.Questions.Add(question);

        await context.SaveChangesAsync();
        var repository = new QuestionRepository(context);
        var result = await repository.GetByIdAsync(question.Id);
        Assert.NotNull(result);
        Assert.Equal("Hitta mig", result?.Text);
    }
}