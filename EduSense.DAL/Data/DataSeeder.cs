using EduSense.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            await SeedIdentityAsync(scope.ServiceProvider);
            await SeedAppDataAsync(scope.ServiceProvider);
        }

        private static async Task SeedIdentityAsync(IServiceProvider services)
        {
            var userContext = services.GetRequiredService<EduSenseUserDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Skapa roller
            foreach (var role in new[] { "Admin", "Analyst" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Skapa admin-användare
            var adminEmail = "admin@edusense.se";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Admin User",
                    IsActive = true
                };

                await userManager.CreateAsync(adminUser, "AdminPw123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Skapa analyst-användare
            var analystEmail = "analyst@edusense.se";
            var analystUser = await userManager.FindByEmailAsync(analystEmail);

            if (analystUser is null)
            {
                analystUser = new ApplicationUser
                {
                    UserName = analystEmail,
                    Email = analystEmail,
                    EmailConfirmed = true,
                    DisplayName = "Analyst User",
                    IsActive = true
                };

                await userManager.CreateAsync(analystUser, "AnalystPw123!");
                await userManager.AddToRoleAsync(analystUser, "Analyst");
            }
        }

        private static async Task SeedAppDataAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<EduSenseDbContext>();

            // Organisationer
            var org1 = new OrganisationModel { Name = "EduSense AB" };
            var org2 = new OrganisationModel { Name = "Test Organisation" };

            context.Organisations.AddRange(org1, org2);
            await context.SaveChangesAsync();

            // Frågor
            var q1 = new QuestionModel
            {
                Text = "Hur nöjd är du med tjänsten?",
                CreatedByUserId = "admin@edusense.se"
            };
            var q2 = new QuestionModel
            {
                Text = "Skulle du rekommendera oss?",
                CreatedByUserId = "admin@edusense.se"
            };

            context.Questions.AddRange(q1, q2);
            await context.SaveChangesAsync();

            // Svaralternativ
            var ans1 = new AnswerOptionModel { Description = "Mycket nöjd", Value = 5 };
            var ans2 = new AnswerOptionModel { Description = "Nöjd", Value = 4 };
            var ans3 = new AnswerOptionModel { Description = "Neutral", Value = 3 };
            var ans4 = new AnswerOptionModel { Description = "Missnöjd", Value = 2 };
            var ans5 = new AnswerOptionModel { Description = "Mycket missnöjd", Value = 1 };

            context.AnswerOptions.AddRange(ans1, ans2, ans3, ans4, ans5);
            await context.SaveChangesAsync();

            // Länka frågor och svaralternativ
            context.QuestionAnswerOptions.AddRange(
                new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans1.Id },
                new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans2.Id },
                new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans3.Id },
                new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans4.Id },
                new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans5.Id }
            );
            await context.SaveChangesAsync();

            // Enkät
            var survey1 = new SurveyModel
            {
                Title = "Kundnöjdhetsenkät",
                CreatedByUserId = "admin@edusense.se",
                SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                OrganisationId = org1.Id
            };

            context.Surveys.Add(survey1);
            await context.SaveChangesAsync();

            // Länka frågor till enkät
            context.SurveyQuestions.AddRange(
                new SurveyQuestionModel { SurveyId = survey1.Id, QuestionId = q1.Id },
                new SurveyQuestionModel { SurveyId = survey1.Id, QuestionId = q2.Id }
            );
            await context.SaveChangesAsync();

            // Respondenter
            var respondent1 = new RespondentModel
            {
                Email = "respondent1@test.com",
                Token = "token-123",
                SurveyId = survey1.Id,
                TokenIsUsed = false
            };
            var respondent2 = new RespondentModel
            {
                Email = "respondent2@test.com",
                Token = "token-456",
                SurveyId = survey1.Id,
                TokenIsUsed = false
            };

            context.Respondents.AddRange(respondent1, respondent2);
            await context.SaveChangesAsync();
        }
    }
}
