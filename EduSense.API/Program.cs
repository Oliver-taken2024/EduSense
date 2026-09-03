using EduSense.DAL.Data;
using EduSense.DAL.Repositories;
using EduSense.BLL.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

static string ToDirectConnectionString(string pooledConnectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(pooledConnectionString)
    {
        Host = new NpgsqlConnectionStringBuilder(pooledConnectionString).Host?.Replace("-pooler", "")
    };
    return builder.ConnectionString;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<EduSenseDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<EduSenseUserDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EduSenseUserDbContext>();

builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IOrganisationRepository, OrganisationRepository>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
        .WithOrigins(
            "https://localhost:7289",
            "http://localhost:5107")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduSenseDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddDbContext<EduSenseUserDbContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!)),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var directConnectionString = ToDirectConnectionString(connectionString!);

    var appDbOptions = new DbContextOptionsBuilder<EduSenseDbContext>()
        .UseNpgsql(directConnectionString).Options;
    var userDbOptions = new DbContextOptionsBuilder<EduSenseUserDbContext>()
        .UseNpgsql(directConnectionString).Options;

    await using var directAppDb = new EduSenseDbContext(appDbOptions);
    await using var directUserDb = new EduSenseUserDbContext(userDbOptions);

    if (app.Environment.IsDevelopment())
        //Skapa om databasen enligt modellerna
    {
        // Samma fysiska databas delas av båda context-klasserna, så EnsureCreatedAsync
        // (som bara kollar "finns databasen") skulle hoppa över Identity-tabellerna
        // eftersom databasen redan finns efter directAppDb:s EnsureCreatedAsync.
        // CreateTablesAsync tvingar fram tabellerna oavsett.
        await directAppDb.Database.EnsureDeletedAsync();
        await directAppDb.Database.EnsureCreatedAsync();
        await directUserDb.Database.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
    }
    else
    //Om production mode, kör migrationer som ev inte är körda
    {
        await directAppDb.Database.MigrateAsync();
        await directUserDb.Database.MigrateAsync();
    }
    //Lägg in seedningsdatat
    await DataSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

app.UseCors("AllowUI");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
