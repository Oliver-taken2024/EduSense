using EduSense.DAL.Data;
using EduSense.DAL.Models;
using EduSense.DAL.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EduSense.DAL.Test.DbContexts;

public class EduSenseUserDbContextTests
{
    [Fact]
    public async Task Can_save_application_user()
    {
        //Skapa db-context mot SQLite-db i minnet
        using var scope = TestDbContextFactory.CreateUserContext();
        var context = scope.Context;

        //lägg till en user
        context.Users.Add(new ApplicationUser
        {
            UserName = "admin@edusense.local",
            Email = "admin@edusense.local",
            DisplayName = "Admin",
            IsActive = true
        });

        await context.SaveChangesAsync();

        var savedUser = await context.Users.SingleAsync();

        //kolla att rätt DisplayName och IsActive blev sparade
        Assert.Equal("Admin", savedUser.DisplayName);
        Assert.True(savedUser.IsActive);
    }
}
