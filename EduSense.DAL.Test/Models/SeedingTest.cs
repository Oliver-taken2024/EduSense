using EduSense.DAL.Data;
using EduSense.DAL.Models;
using EduSense.DAL.Test.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduSense.DAL.Test.Models;

public class SeedingTest
{
    [Fact]
    public async Task SeedAsync_creates_roles_and_users()
    {
        var serviceProvider = TestSeederServiceProviderFactory.CreateServiceProvider();

        await DataSeeder.SeedAsync(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Kontrollera att roller skapades
        Assert.True(await roleManager.RoleExistsAsync("Admin"));
        Assert.True(await roleManager.RoleExistsAsync("Analyst"));

        // Kontrollera att admin-användare skapades
        var adminUser = await userManager.FindByEmailAsync("admin@edusense.se");
        Assert.NotNull(adminUser);
        Assert.True(adminUser.IsActive);
        Assert.Equal("Admin User", adminUser.DisplayName);

        // Kontrollera att admin har Admin-roll
        var adminRoles = await userManager.GetRolesAsync(adminUser);
        Assert.Contains("Admin", adminRoles);

        // Kontrollera att analyst-användare skapades
        var analystUser = await userManager.FindByEmailAsync("analyst@edusense.se");
        Assert.NotNull(analystUser);
        Assert.True(analystUser.IsActive);

        var analystRoles = await userManager.GetRolesAsync(analystUser);
        Assert.Contains("Analyst", analystRoles);
    }

    [Fact]
    public async Task SeedAsync_creates_expected_app_data()
    {
        var serviceProvider = TestSeederServiceProviderFactory.CreateServiceProvider();

        await DataSeeder.SeedAsync(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var verifyAppContext = scope.ServiceProvider.GetRequiredService<EduSenseDbContext>();

        // Kontrollera organisationer
        var organisations = await verifyAppContext.Organisations.ToListAsync();
        Assert.NotEmpty(organisations);
        Assert.Contains(organisations, o => o.Name == "EduSense AB");

        // Kontrollera frågor
        var questions = await verifyAppContext.Questions.ToListAsync();
        Assert.NotEmpty(questions);
        Assert.True(questions.Count >= 2);

        // Kontrollera svaralternativ
        var answerOptions = await verifyAppContext.AnswerOptions.ToListAsync();
        Assert.NotEmpty(answerOptions);

        // Kontrollera enkät
        var surveys = await verifyAppContext.Surveys.ToListAsync();
        Assert.NotEmpty(surveys);
        Assert.Contains(surveys, s => s.Title == "Kundnöjdhetsenkät");

        // Kontrollera respondenter
        var respondents = await verifyAppContext.Respondents.ToListAsync();
        Assert.NotEmpty(respondents);
        Assert.True(respondents.All(r => !r.TokenIsUsed));
    }

    [Fact]
    public async Task SeedAsync_is_idempotent_when_called_twice()
    {
        var serviceProvider = TestSeederServiceProviderFactory.CreateServiceProvider();

        // Första seedning
        await DataSeeder.SeedAsync(serviceProvider);

        // Andra seedning
        await DataSeeder.SeedAsync(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var verifyAppContext = scope.ServiceProvider.GetRequiredService<EduSenseDbContext>();

        // Kontrollera att det bara finns en Admin-roll (inte duplicerade)
        var adminRole = await roleManager.FindByNameAsync("Admin");
        Assert.NotNull(adminRole);

        // Kontrollera att det bara finns en admin-användare
        var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
        Assert.Single(adminUsers);
        Assert.Equal("admin@edusense.se", adminUsers[0].Email);

        // Kontrollera att organisationer inte duplicerades
        var orgCount = await verifyAppContext.Organisations
            .Where(o => o.Name == "EduSense AB")
            .CountAsync();
        Assert.Equal(1, orgCount);

        // Kontrollera att enkäter inte duplicerades
        var surveyCount = await verifyAppContext.Surveys
            .Where(s => s.Title == "Kundnöjdhetsenkät")
            .CountAsync();
        Assert.Equal(1, surveyCount);

        // Kontrollera att respondenter inte duplicerades
        var respondentCount = await verifyAppContext.Respondents
            .Where(r => r.Email == "respondent1@test.com")
            .CountAsync();
        Assert.Equal(1, respondentCount);
    }
}
