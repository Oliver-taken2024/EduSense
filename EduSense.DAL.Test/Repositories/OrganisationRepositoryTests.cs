using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduSense.DAL.Test.Repositories;

public class OrganisationRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_returns_all_seeded_organisations()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        context.Organisations.Add(new OrganisationModel { Name = "EduSense AB" });
        context.Organisations.Add(new OrganisationModel { Name = "Test Organisation" });
        await context.SaveChangesAsync();

        var repository = new OrganisationRepository(context);
        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_returns_organisation_when_exists()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        var organisation = new OrganisationModel { Name = "EduSense AB" };
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        var repository = new OrganisationRepository(context);
        var result = await repository.GetByIdAsync(organisation.Id);

        Assert.NotNull(result);
        Assert.Equal("EduSense AB", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_not_exists()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var repository = new OrganisationRepository(scope.Context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_persists_organisation()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;
        var repository = new OrganisationRepository(context);

        var created = await repository.CreateAsync(new OrganisationModel { Name = "Ny Organisation" });

        Assert.True(created.Id > 0);
        Assert.Equal(1, await context.Organisations.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_modifies_organisation_name()
    {
        using var scope = TestDbContextFactory.CreateAppContext();
        var context = scope.Context;

        var organisation = new OrganisationModel { Name = "Gammalt Namn" };
        context.Organisations.Add(organisation);
        await context.SaveChangesAsync();

        // Ny context-instans för att simulera en "fristående" uppdaterad entitet
        organisation.Name = "Nytt Namn";
        var repository = new OrganisationRepository(context);
        var updated = await repository.UpdateAsync(organisation);

        Assert.Equal("Nytt Namn", updated.Name);
        var fromDb = await context.Organisations.AsNoTracking().SingleAsync(o => o.Id == organisation.Id);
        Assert.Equal("Nytt Namn", fromDb.Name);
    }
}