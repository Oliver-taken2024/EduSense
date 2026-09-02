using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace EduSense.DAL.Test.Helpers;

//hjälpklass för att hålla db connection igång under testen och inte stängs för tidigt och kastar den efter fullgjort test.

public sealed class TestDbContextScope<TContext> : IDisposable
    where TContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly SqliteConnection _connection;

    public TestDbContextScope(SqliteConnection connection, TContext context)
    {
        _connection = connection;
        Context = context;
    }

    public TContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
