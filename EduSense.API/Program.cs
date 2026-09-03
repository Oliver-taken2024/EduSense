using EduSense.DAL.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduSenseDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddDbContext<EduSenseUserDbContext>(o => o.UseNpgsql(connectionString));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var appDb = scope.ServiceProvider.GetRequiredService<EduSenseDbContext>();
    var userDb = scope.ServiceProvider.GetRequiredService<EduSenseUserDbContext>();

    if (app.Environment.IsDevelopment())
        //Skapa om databasen enligt modellerna
    {
        await appDb.Database.EnsureDeletedAsync();
        await userDb.Database.EnsureDeletedAsync();
        await appDb.Database.EnsureCreatedAsync();
        await userDb.Database.EnsureCreatedAsync();
    }
    else
    //Om production mode, kör migrationer som ev inte är körda
    {
        await appDb.Database.MigrateAsync();
        await userDb.Database.MigrateAsync();
    }
    //Lägg in seedningsdatat
    await DataSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
