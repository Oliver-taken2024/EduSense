using EduSense.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace EduSense.DAL.Test.Helpers;

//Skapa en app-/identity-context för SQL-lite test-databasen som skapas i minnet
public static class TestDbContextFactory
{
    public static TestDbContextScope<EduSenseDbContext> CreateAppContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<EduSenseDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new EduSenseDbContext(options);
        context.Database.EnsureCreated();

        return new TestDbContextScope<EduSenseDbContext>(connection, context);
    }

    public static TestDbContextScope<EduSenseUserDbContext> CreateUserContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<EduSenseUserDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new EduSenseUserDbContext(options);
        context.Database.EnsureCreated();

        return new TestDbContextScope<EduSenseUserDbContext>(connection, context);
    }
}
