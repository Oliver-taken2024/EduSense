using EduSense.DAL.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // ger RemoveAll<T>()
using Microsoft.EntityFrameworkCore.Storage; // IRelationalDatabaseCreator
using EduSense.BLL.Services;

namespace EduSense.API.Test.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Hålls öppen hela factoryns livstid - stängs anslutningen försvinner in-memory-databasen.
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<EduSenseDbContext>));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<EduSenseDbContext>));
                services.RemoveAll(typeof(DbContextOptions<EduSenseUserDbContext>));
                services.RemoveAll(typeof(IDbContextOptionsConfiguration<EduSenseUserDbContext>));
                services.RemoveAll(typeof(IEmailSender));
                services.AddScoped<IEmailSender, NullEmailSender>();

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
