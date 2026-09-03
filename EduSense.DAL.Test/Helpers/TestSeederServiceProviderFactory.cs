using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;


namespace EduSense.DAL.Test.Helpers;

public static class TestSeederServiceProviderFactory
{
    private static SqliteConnection? _sharedConnection;

    public static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Ny connection PER testkörning (isolerad, inte delad mellan tester)
        var dbPath = Path.Combine(Path.GetTempPath(), $"edusense_test_{Guid.NewGuid()}.db");
        var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        // Registrera EduSenseDbContext
        services.AddDbContext<EduSenseDbContext>(options =>
            options.UseSqlite(connection)
        );

        // Registrera EduSenseUserDbContext
        services.AddDbContext<EduSenseUserDbContext>(options =>
            options.UseSqlite(connection)
        );

        // Registrera Identity
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<EduSenseUserDbContext>();

        var serviceProvider = services.BuildServiceProvider();

        // Skapa databaser
        using var scope = serviceProvider.CreateScope();
        
            var userContext = scope.ServiceProvider.GetRequiredService<EduSenseUserDbContext>();
            var appContext = scope.ServiceProvider.GetRequiredService<EduSenseDbContext>();
           

            userContext.Database.EnsureCreated();
            appContext.GetService<IRelationalDatabaseCreator>().CreateTables();



        return serviceProvider;
    }
}
