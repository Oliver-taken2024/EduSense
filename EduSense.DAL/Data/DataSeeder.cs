using EduSense.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
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
            var org1 = await context.Organisations
                .SingleOrDefaultAsync(o => o.Name == "EduSense AB");

            if (org1 is null)
            {
                org1 = new OrganisationModel { Name = "EduSense AB" };
                context.Organisations.Add(org1);
            }

            var org2 = await context.Organisations
                .SingleOrDefaultAsync(o => o.Name == "Test Organisation");

            if (org2 is null)
            {
                org2 = new OrganisationModel { Name = "Test Organisation" };
                context.Organisations.Add(org2);
            }

            await context.SaveChangesAsync();

            // Frågor
            var q1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Hur nöjd är du med tjänsten?");
            if (q1 is null)
            {
                q1 = new QuestionModel { Text = "Hur nöjd är du med tjänsten?", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q1);
            }

            var q2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Skulle du rekommendera oss?");
            if (q2 is null)
            {
                q2 = new QuestionModel { Text = "Skulle du rekommendera oss?", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q2);
            }


            await context.SaveChangesAsync();

            // Svarsalternativ
            var ans1 = await context.AnswerOptions.SingleOrDefaultAsync(x => x.Description == "Mycket nöjd" && x.Value == 5);
            if (ans1 is null)
            {
                ans1 = new AnswerOptionModel { Description = "Mycket nöjd", Value = 5 };
                context.AnswerOptions.Add(ans1);
            }

            var ans2 = await context.AnswerOptions.SingleOrDefaultAsync(x => x.Description == "Nöjd" && x.Value == 4);
            if (ans2 is null)
            {
                ans2 = new AnswerOptionModel { Description = "Nöjd", Value = 4 };
                context.AnswerOptions.Add(ans2);
            }

            var ans3 = await context.AnswerOptions.SingleOrDefaultAsync(x => x.Description == "Neutral" && x.Value == 3);
            if (ans3 is null)
            {
                ans3 = new AnswerOptionModel { Description = "Neutral", Value = 3 };
                context.AnswerOptions.Add(ans3);
            }

            var ans4 = await context.AnswerOptions.SingleOrDefaultAsync(x => x.Description == "Missnöjd" && x.Value == 2);
            if (ans4 is null)
            {
                ans4 = new AnswerOptionModel { Description = "Missnöjd", Value = 2 };
                context.AnswerOptions.Add(ans4);
            }

            var ans5 = await context.AnswerOptions.SingleOrDefaultAsync(x => x.Description == "Mycket missnöjd" && x.Value == 1);
            if (ans5 is null)
            {
                ans5 = new AnswerOptionModel { Description = "Mycket missnöjd", Value = 1 };
                context.AnswerOptions.Add(ans5);
            }

            await context.SaveChangesAsync();

            // Länka frågor och svarsalternativ
            if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == q1.Id && x.AnswerOptionId == ans1.Id))
            {
                context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans1.Id });
            }
            if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == q1.Id && x.AnswerOptionId == ans2.Id))
            {
                context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans2.Id });
            }
            if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == q1.Id && x.AnswerOptionId == ans3.Id))
            {
                context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans3.Id });
            }
            if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == q1.Id && x.AnswerOptionId == ans4.Id))
            {
                context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans4.Id });
            }
            if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == q1.Id && x.AnswerOptionId == ans5.Id))
            {
                context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = q1.Id, AnswerOptionId = ans5.Id });
            }

            await context.SaveChangesAsync();

            // Enkät
            var survey1 = await context.Surveys
                .SingleOrDefaultAsync(x => x.Title == "Kundnöjdhetsenkät" && x.OrganisationId == org1.Id);
            if (survey1 is null)
            {
                survey1 = new SurveyModel
                {
                    Title = "Kundnöjdhetsenkät",
                    CreatedByUserId = "admin@edusense.se",
                    SurveyExpiryDate = DateTime.UtcNow.AddDays(30),
                    OrganisationId = org1.Id
                };

                context.Surveys.Add(survey1);
                await context.SaveChangesAsync();
            }

            // Länka frågor till enkät
            if (!await context.SurveyQuestions.AnyAsync(x => x.SurveyId == survey1.Id && x.QuestionId == q1.Id))
            {
                context.SurveyQuestions.Add(new SurveyQuestionModel { SurveyId = survey1.Id, QuestionId = q1.Id });
            }
            if (!await context.SurveyQuestions.AnyAsync(x => x.SurveyId == survey1.Id && x.QuestionId == q2.Id))
            {
                context.SurveyQuestions.Add(new SurveyQuestionModel { SurveyId = survey1.Id, QuestionId = q2.Id });
            }
            await context.SaveChangesAsync();

            // Respondenter
            if (!await context.Respondents.AnyAsync(x => x.Email == "respondent1@test.com" && x.SurveyId == survey1.Id))
            {
                context.Respondents.Add(new RespondentModel
                {
                    Email = "respondent1@test.com",
                    Token = "token-123",
                    SurveyId = survey1.Id,
                    TokenIsUsed = false
                });
            }
            if (!await context.Respondents.AnyAsync(x => x.Email == "respondent2@test.com" && x.SurveyId == survey1.Id))
            {
                context.Respondents.Add(new RespondentModel
                {
                    Email = "respondent2@test.com",
                    Token = "token-456",
                    SurveyId = survey1.Id,
                    TokenIsUsed = false
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
