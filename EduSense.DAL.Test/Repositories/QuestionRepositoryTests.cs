using EduSense.DAL.Data;
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
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

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

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

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
        Assert.Equal(1, await context.Questions.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateAsync_links_standard_five_point_answer_scale()
    {
        // Utan den här kopplingen har en nyskapad fråga inga svarsalternativ alls,
        // och respondenter kan då inte välja något på frågesidan.
        // Standardskalan seedas här precis som DataSeeder gör i produktion - CreateAsync
        // slår bara upp den, den skapar den aldrig själv.
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        SeedStandardAnswerScale(context);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new QuestionRepository(context);

        var created = await repository.CreateAsync(new QuestionModel
        {
            Text = "Ny fråga",
            CreatedByUserId = "user-1"
        });

        var linkedValues = await context.QuestionAnswerOptions
            .Where(x => x.QuestionId == created.Id)
            .Include(x => x.AnswerOption)
            .Select(x => x.AnswerOption!.Value)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal([1, 2, 3, 4, 5], linkedValues.OrderBy(v => v));
    }

    [Fact]
    public async Task CreateAsync_reuses_existing_answer_options_instead_of_duplicating()
    {
        // Två frågor ska dela samma 5 AnswerOption-rader, inte skapa nya dubbletter varje gång.
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        SeedStandardAnswerScale(context);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new QuestionRepository(context);

        await repository.CreateAsync(new QuestionModel { Text = "Fråga A", CreatedByUserId = "user-1" });
        await repository.CreateAsync(new QuestionModel { Text = "Fråga B", CreatedByUserId = "user-1" });

        Assert.Equal(5, await context.AnswerOptions.CountAsync(TestContext.Current.CancellationToken));
    }

    // Samma fem rader som DataSeeder seedar i produktion - QuestionRepository.CreateAsync
    // förutsätter att de redan finns och skapar dem aldrig själv.
    private static void SeedStandardAnswerScale(EduSenseDbContext context)
    {
        context.AnswerOptions.AddRange(
            new AnswerOptionModel { Description = "Mycket missnöjd", Value = 1, ScaleType = AnswerScaleType.Standard1To5 },
            new AnswerOptionModel { Description = "Missnöjd", Value = 2, ScaleType = AnswerScaleType.Standard1To5 },
            new AnswerOptionModel { Description = "Neutral", Value = 3, ScaleType = AnswerScaleType.Standard1To5 },
            new AnswerOptionModel { Description = "Nöjd", Value = 4, ScaleType = AnswerScaleType.Standard1To5 },
            new AnswerOptionModel { Description = "Mycket nöjd", Value = 5, ScaleType = AnswerScaleType.Standard1To5 });
    }

    // Samma 10 rader som DataSeeder seedar åt NPS-frågan i produktion.
    private static void SeedNpsAnswerScale(EduSenseDbContext context)
    {
        for (var value = 1; value <= 10; value++)
        {
            context.AnswerOptions.Add(new AnswerOptionModel
            {
                Description = $"NPS: {value}",
                Value = value,
                ScaleType = AnswerScaleType.Nps1To10
            });
        }
    }

    [Fact]
    public async Task CreateAsync_with_NpsScaleType_links_ten_point_nps_scale_instead_of_standard()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        SeedStandardAnswerScale(context);
        SeedNpsAnswerScale(context);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var repository = new QuestionRepository(context);

        var created = await repository.CreateAsync(
            new QuestionModel { Text = "Hur sannolikt är det att du rekommenderar oss?", CreatedByUserId = "user-1" },
            AnswerScaleType.Nps1To10);

        var linkedValues = await context.QuestionAnswerOptions
            .Where(x => x.QuestionId == created.Id)
            .Include(x => x.AnswerOption)
            .Select(x => x.AnswerOption!.Value)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Enumerable.Range(1, 10), linkedValues.OrderBy(v => v));
    }

    [Fact]
    public async Task DeleteAsync_removes_question()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        var question = new QuestionModel { Text = "Ta bort mig", CreatedByUserId = "user-1" };
        context.Questions.Add(question);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

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

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var repository = new QuestionRepository(context);
        question.Text = "Jag är uppdaterad";

        await repository.UpdateAsync(question);
        var updated = await context.Questions.FirstOrDefaultAsync(q => q.Id == question.Id, TestContext.Current.CancellationToken);
        Assert.Equal("Jag är uppdaterad", updated?.Text);
    }

    [Fact]
    public async Task GetByIdAsync_returns_question_when_exists()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        var question = new QuestionModel { Text = "Hitta mig", CreatedByUserId = "user-1" };
        context.Questions.Add(question);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var repository = new QuestionRepository(context);
        var result = await repository.GetByIdAsync(question.Id);
        Assert.NotNull(result);
        Assert.Equal("Hitta mig", result?.Text);
    }
}