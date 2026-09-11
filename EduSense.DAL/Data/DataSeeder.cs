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
        // SeedAsync-metoden skapar en scope och anropar metoderna
        // för att seed:a identitet och applikationsdata.
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            await SeedIdentityAsync(scope.ServiceProvider);
            await SeedAppDataAsync(scope.ServiceProvider);
        }

        private static async Task SeedIdentityAsync(IServiceProvider services)
        {
            // Hämtar UserManager, RoleManager och EduSenseUserDbContext från dependency injection
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
            var adminEmail = "admin@edusense.com";
            var adminUserName = "Admin";
            var adminUser = await userManager.FindByNameAsync(adminUserName);

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Admin User",
                    IsActive = true
                };

                await userManager.CreateAsync(adminUser, "AdminPw123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Skapa analyst-användare
            var analystEmail = "analyst@edusense.com";
            var analystUserName = "Analyst";
            var analystUser = await userManager.FindByNameAsync(analystUserName);

            if (analystUser is null)
            {
                analystUser = new ApplicationUser
                {
                    UserName = analystUserName,
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
            var q1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Hur nöjd är du med skolans verksamhet överlag?");
            if (q1 is null)
            {
                q1 = new QuestionModel { Text = "Hur nöjd är du med skolans verksamhet överlag?", CreatedByUserId = "admin@edusense.com" };
                context.Questions.Add(q1);
            }

            var q2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Skulle du rekommendera oss?");
            if (q2 is null)
            {
                q2 = new QuestionModel { Text = "Skulle du rekommendera oss?", CreatedByUserId = "admin@edusense.com" };
                context.Questions.Add(q2);
            }

            var q3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag får bra information om vad som händer på skolan");
            if (q3 is null)
            {
                q3 = new QuestionModel { Text = "Jag får bra information om vad som händer på skolan", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q3);
            }


            var q4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Inomhusmiljön är stimulerande och trivsam");
            if (q4 is null)
            {
                q4 = new QuestionModel { Text = "Inomhusmiljön är stimulerande och trivsam", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q4);
            }

            var q5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Utomhusmiljön är stimulerande för mitt barn");
            if (q5 is null)
            {
                q5 = new QuestionModel { Text = "Utomhusmiljön är stimulerande för mitt barn", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q5);
            }

            var q6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att mitt barn äter skolmaten");
            if (q6 is null)
            {
                q6 = new QuestionModel { Text = "Jag upplever att mitt barn äter skolmaten", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q6);
            }

            var q7 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Fritidshemmet erbjuder en utvecklande verksamhet för mitt barn");
            if (q7 is null)
            {
                q7 = new QuestionModel { Text = "Fritidshemmet erbjuder en utvecklande verksamhet för mitt barn", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q7);
            }

            var q8 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att mitt barn trivs på fritids");
            if (q8 is null)
            {
                q8 = new QuestionModel { Text = "Jag upplever att mitt barn trivs på fritids", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q8);
            }

            var q9 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att skolan arbetar aktivt mot diskriminering och trakasserier");
            if (q9 is null)
            {
                q9 = new QuestionModel { Text = "Jag upplever att skolan arbetar aktivt mot diskriminering och trakasserier", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q9);
            }

            var q10 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Mitt barn får den arbetsro hen behöver");
            if (q10 is null)
            {
                q10 = new QuestionModel { Text = "Mitt barn får den arbetsro hen behöver", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q10);
            }

            var q11 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag känner mig trygg med personalens tillsyn över eleverna");
            if (q11 is null)
            {
                q11 = new QuestionModel { Text = "Jag känner mig trygg med personalens tillsyn över eleverna", CreatedByUserId = "admin@edusense.se" };
                context.Questions.Add(q11);
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

            // Länka alla frågor till alla svarsalternativ (1-5-skala på varje fråga).
            var allQuestions = new[] { q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11 };
            var allAnswerOptions = new[] { ans1, ans2, ans3, ans4, ans5 };

            foreach (var question in allQuestions)
            {
                foreach (var answerOption in allAnswerOptions)
                {
                    if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == question.Id && x.AnswerOptionId == answerOption.Id))
                    {
                        context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = question.Id, AnswerOptionId = answerOption.Id });
                    }
                }
            }

            await context.SaveChangesAsync();

            // Enkät 1 - alla 11 frågor
            var survey1 = await EnsureSurveyAsync(context, "Kundnöjdhetsenkät", org1.Id, DateTime.UtcNow.AddDays(30));
            await LinkQuestionsToSurveyAsync(context, survey1, allQuestions);
            await EnsureRespondentAsync(context, survey1, "respondent1@test.com", "token-123");
            await EnsureRespondentAsync(context, survey1, "respondent2@test.com", "token-456");

            // Enkät 2 - föräldraenkät, skol-/fritidsrelaterade frågor
            var survey2 = await EnsureSurveyAsync(context, "Föräldraenkät - skola och fritids", org1.Id, DateTime.UtcNow.AddDays(14));
            await LinkQuestionsToSurveyAsync(context, survey2, [q3, q4, q5, q6, q7, q8, q9, q10, q11]);
            await EnsureRespondentAsync(context, survey2, "respondent3@test.com", "token-789");
            await EnsureRespondentAsync(context, survey2, "respondent4@test.com", "token-101");

            // Enkät 3 - utgången, för att testa expired-flödet utan att vänta
            var survey3 = await EnsureSurveyAsync(context, "Trivselenkät (utgången)", org2.Id, DateTime.UtcNow.AddDays(-5));
            await LinkQuestionsToSurveyAsync(context, survey3, [q1, q2, q3]);
            await EnsureRespondentAsync(context, survey3, "respondent5@test.com", "token-expired");
        }

        private static async Task<SurveyModel> EnsureSurveyAsync(EduSenseDbContext context, string title, int organisationId, DateTime expiryDate)
        {
            var survey = await context.Surveys.SingleOrDefaultAsync(x => x.Title == title && x.OrganisationId == organisationId);
            if (survey is null)
            {
                survey = new SurveyModel
                {
                    Title = title,
                    CreatedByUserId = "admin@edusense.com",
                    SurveyExpiryDate = expiryDate,
                    OrganisationId = organisationId
                };

                context.Surveys.Add(survey);
                await context.SaveChangesAsync();
            }

            return survey;
        }

        private static async Task LinkQuestionsToSurveyAsync(EduSenseDbContext context, SurveyModel survey, IEnumerable<QuestionModel> questions)
        {
            foreach (var question in questions)
            {
                if (!await context.SurveyQuestions.AnyAsync(x => x.SurveyId == survey.Id && x.QuestionId == question.Id))
                {
                    context.SurveyQuestions.Add(new SurveyQuestionModel { SurveyId = survey.Id, QuestionId = question.Id });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureRespondentAsync(EduSenseDbContext context, SurveyModel survey, string email, string token)
        {
            if (!await context.Respondents.AnyAsync(x => x.Email == email && x.SurveyId == survey.Id))
            {
                context.Respondents.Add(new RespondentModel
                {
                    Email = email,
                    Token = token,
                    SurveyId = survey.Id,
                    TokenIsUsed = false
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
