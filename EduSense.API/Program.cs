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
using EduSense.Infrastructure.Email;
using EduSense.Infrastructure.Ai;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;


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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EduSenseDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDbContext<EduSenseUserDbContext>(options => options.UseNpgsql(connectionString));

// Delad nyckelring i databasen istället för lokal fil per maskin - annars kan en
// utvecklares API-instans inte validera en invite-/reset-token som skapats av en
// annan utvecklares API-instans ("invalid token" trots korrekt token).
// SetApplicationName måste vara identiskt på alla instanser (annars särnycklar
// nyckelringen per app-namn, som annars härleds från content root-sökvägen och
// skiljer sig mellan maskiner/klonade repo-sökvägar).
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<EduSenseUserDbContext>()
    .SetApplicationName("EduSense");

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddSignInManager()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EduSenseUserDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
});

builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<ISurveyService, SurveyService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IOrganisationRepository, OrganisationRepository>();
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped <IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IRespondentRepository, RespondentRepository>();
builder.Services.AddScoped<IRespondentService, RespondentService>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<ISurveyDispatchRepository, SurveyDispatchRepository>();
builder.Services.AddScoped<ISurveyDispatchService, SurveyDispatchService>();
builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));
builder.Services.AddHttpClient<IOllamaClient, OllamaClient>(client =>
{
    // Konfigurerbar istället för hårdkodad 2 min - mistral:7b (CPU) kan ta längre
    // tid än så, se OllamaOptions.TimeoutSeconds (default 300s / 5 min).
    var timeoutSeconds = builder.Configuration.GetValue("Ollama:TimeoutSeconds", 300);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});
builder.Services.AddScoped<ISurveyAiService, SurveyAiService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
        .WithOrigins(
            "https://localhost:7289",
            "http://localhost:5107")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("AnalystOnly", policy => policy.RequireRole("Analyst"))
    .AddPolicy("AdminOrAnalyst", policy => policy.RequireRole("Admin", "Analyst"));

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
         options.Events = new JwtBearerEvents
         {
        OnMessageReceived = context =>
        {
            context.Token =
                context.Request.Cookies["accessToken"];

            return Task.CompletedTask;
        }
        };
    });

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromDays(1);
});

var emailProvider = builder.Configuration["Email:Provider"] ?? "Sandbox";
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection($"Smtp:{emailProvider}"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<IEmailSender, NullEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.UseSwagger();
    app.UseSwaggerUI();
}

if(!app.Environment.IsEnvironment("Testing"))
{
   
    using (var scope = app.Services.CreateScope())
    {
        var directConnectionString = ToDirectConnectionString(connectionString!);

        var appDbOptions = new DbContextOptionsBuilder<EduSenseDbContext>()
            .UseNpgsql(directConnectionString).Options;
        var userDbOptions = new DbContextOptionsBuilder<EduSenseUserDbContext>()
            .UseNpgsql(directConnectionString).Options;

        await using var directAppDb = new EduSenseDbContext(appDbOptions);
        await using var directUserDb = new EduSenseUserDbContext(userDbOptions);

        if (args.Contains("--reset-db"))
        // Nollställ delad Neon-DB - körs bara manuellt, aldrig automatiskt
        {
            await directAppDb.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE;");
            await directAppDb.Database.ExecuteSqlRawAsync("CREATE SCHEMA public;");
        }

        // Kör ev. ej applicerade migrationer
        await directAppDb.Database.MigrateAsync();
        await directUserDb.Database.MigrateAsync();

        if (args.Contains("--seed"))
        //Lägg in seedningsdatat
        {
            await DataSeeder.SeedAsync(app.Services);
        }
    }

}

app.UseHttpsRedirection();

app.UseCors("AllowUI");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//Detta krävs för att WebApplicationFactory<Program> ska kunna referera till entry point-klassen — 
//top-level statements genererar annars en internal klass som testprojektet inte kommer åt.
public partial class Program { }
