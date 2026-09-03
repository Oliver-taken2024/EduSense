using EduSense.DAL.Data;
using EduSense.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<EduSenseDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<EduSenseUserDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
        .WithOrigins(
            "https://localhost:7271",
            "http://localhost:5227")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduSenseDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddDbContext<EduSenseUserDbContext>(o => o.UseNpgsql(connectionString));

//aktivera ASP.NET Identity användar- och rollhantering
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EduSenseUserDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AnalystOnly", policy => policy.RequireRole("Analyst"));
    options.AddPolicy("RespondentOnly", policy => policy.RequireRole("Respondent"));
    options.AddPolicy("AdminOrAnalyst", policy => policy.RequireRole("Admin", "Analyst"));
});

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

app.UseCors("AllowUI");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
