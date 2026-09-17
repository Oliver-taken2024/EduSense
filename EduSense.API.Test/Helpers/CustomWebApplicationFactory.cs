using EduSense.BLL.Services;
using EduSense.DAL.Data;
using EduSense.Infrastructure.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; 
using Moq;

namespace EduSense.API.Test.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Hålls öppen hela factoryns livstid - stängs anslutningen försvinner in-memory-databasen.
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        // Konfigurera web-host för testning
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<EduSenseDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<EduSenseDbContext>>();
                services.RemoveAll<DbContextOptions<EduSenseUserDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<EduSenseUserDbContext>>();
                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, NullEmailSender>();
                services.RemoveAll<IOllamaClient>();
                services.AddScoped<IOllamaClient>(_ =>
                {
                    var mock = new Mock<IOllamaClient>();
                    mock.Setup(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync("Testsvar från AI");
                    return mock.Object;
                });

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, options => { });

                services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                });

                _connection.Open();

                services.AddDbContext<EduSenseDbContext>(options => options.UseSqlite(_connection));
                services.AddDbContext<EduSenseUserDbContext>(options => options.UseSqlite(_connection));

                using var scope = services.BuildServiceProvider().CreateScope();

                var userDb = scope.ServiceProvider.GetRequiredService<EduSenseUserDbContext>();
                userDb.Database.EnsureCreated();

                var appDb = scope.ServiceProvider.GetRequiredService<EduSenseDbContext>();
                appDb.Database.GetService<IRelationalDatabaseCreator>().CreateTables();

                DataSeeder.SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();

            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Dispose();
        }
    }
}
