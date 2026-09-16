using EduSense.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EduSense.DAL.Data
{
    public static class DataSeeder
    {
        // Fast tidpunkt för de ursprungliga 11 frågorna i frågebanken, så CreatedAt
        // blir deterministiskt vid omkörning istället för DateTime.UtcNow varje gång.
        private static readonly DateTime QuestionBankCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // NPS-frågan lades till senare än ursprungsfrågebanken, egen tidpunkt av samma anledning.
        private static readonly DateTime NpsQuestionCreatedAt = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Tidpunkt för de nya standardenkäterna (medarbetar-, elev- och uppföljningsenkäter), egen tidpunkt av samma anledning som ovan.
        private static readonly DateTime ExpandedSurveysCreatedAt = new(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc);

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
            var analystEmail = "analytiker@edusense.se";
            var analystUserName = "Analytiker";
            var analystUser = await userManager.FindByNameAsync(analystUserName);

            if (analystUser is null)
            {
                analystUser = new ApplicationUser
                {
                    UserName = analystUserName,
                    Email = analystEmail,
                    EmailConfirmed = true,
                    DisplayName = "Analytiker Användare",
                    IsActive = true
                };

                await userManager.CreateAsync(analystUser, "Analytiker123!");
                await userManager.AddToRoleAsync(analystUser, "Analyst");
            }

            // Skapa Oliver Analytiker
            var oliverAnalytikerEmail = "oliveranalytiker@edusense.se";
            var oliverAnalytikerUserName = "OliverAnalytiker";
            var oliverAnalytikerUser = await userManager.FindByNameAsync(oliverAnalytikerUserName);

            if (oliverAnalytikerUser is null)
            {
                oliverAnalytikerUser = new ApplicationUser
                {
                    UserName = oliverAnalytikerUserName,
                    Email = oliverAnalytikerEmail,
                    EmailConfirmed = true,
                    DisplayName = "Oliver Analytiker",
                    IsActive = true
                };

                await userManager.CreateAsync(oliverAnalytikerUser, "Newton123!");
                await userManager.AddToRoleAsync(oliverAnalytikerUser, "Analyst");
            }

            // Skapa Oliver Admin
            var oliverAdminEmail = "oliveradmin@edusense.se";
            var oliverAdminUserName = "OliverAdmin";
            var oliverAdminUser = await userManager.FindByNameAsync(oliverAdminUserName);

            if (oliverAdminUser is null)
            {
                oliverAdminUser = new ApplicationUser
                {
                    UserName = oliverAdminUserName,
                    Email = oliverAdminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Oliver Admin",
                    IsActive = true
                };

                await userManager.CreateAsync(oliverAdminUser, "Newton123!");
                await userManager.AddToRoleAsync(oliverAdminUser, "Admin");
            }

            // Skapa Henrik Admin
            var henrikAdminEmail = "henrikadmin@edusense.se";
            var henrikAdminUserName = "HenrikAdmin";
            var henrikAdminUser = await userManager.FindByNameAsync(henrikAdminUserName);

            if (henrikAdminUser is null)
            {
                henrikAdminUser = new ApplicationUser
                {
                    UserName = henrikAdminUserName,
                    Email = henrikAdminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Henrik Admin",
                    IsActive = true
                };

                await userManager.CreateAsync(henrikAdminUser, "Newton123!");
                await userManager.AddToRoleAsync(henrikAdminUser, "Admin");
            }

            // Skapa Robin Admin
            var robinAdminEmail = "robinadmin@edusense.se";
            var robinAdminUserName = "RobinAdmin";
            var robinAdminUser = await userManager.FindByNameAsync(robinAdminUserName);

            if (robinAdminUser is null)
            {
                robinAdminUser = new ApplicationUser
                {
                    UserName = robinAdminUserName,
                    Email = robinAdminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Robin Admin",
                    IsActive = true
                };

                await userManager.CreateAsync(robinAdminUser, "Newton123!");
                await userManager.AddToRoleAsync(robinAdminUser, "Admin");
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

            // Kategorier
            var catLarande = await context.Categories.SingleOrDefaultAsync(x => x.Name == "Lärande & Utveckling");
            if (catLarande is null)
            {
                catLarande = new CategoryModel { Name = "Lärande & Utveckling" };
                context.Categories.Add(catLarande);
            }

            var catUndervisning = await context.Categories.SingleOrDefaultAsync(x => x.Name == "Undervisning & Innehåll");
            if (catUndervisning is null)
            {
                catUndervisning = new CategoryModel { Name = "Undervisning & Innehåll" };
                context.Categories.Add(catUndervisning);
            }

            var catStod = await context.Categories.SingleOrDefaultAsync(x => x.Name == "Stöd & Bemötande");
            if (catStod is null)
            {
                catStod = new CategoryModel { Name = "Stöd & Bemötande" };
                context.Categories.Add(catStod);
            }

            var catOrganisation = await context.Categories.SingleOrDefaultAsync(x => x.Name == "Organisation");
            if (catOrganisation is null)
            {
                catOrganisation = new CategoryModel { Name = "Organisation" };
                context.Categories.Add(catOrganisation);
            }

            var catLokaler = await context.Categories.SingleOrDefaultAsync(x => x.Name == "Lokaler & Miljö");
            if (catLokaler is null)
            {
                catLokaler = new CategoryModel { Name = "Lokaler & Miljö" };
                context.Categories.Add(catLokaler);
            }

            await context.SaveChangesAsync();

            // Frågor
            var q1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Hur nöjd är du med skolans verksamhet överlag?");
            if (q1 is null)
            {
                q1 = new QuestionModel { Text = "Hur nöjd är du med skolans verksamhet överlag?", CreatedByUserId = "admin@edusense.com", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q1);
            }

            var q2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Skulle du rekommendera oss?");
            if (q2 is null)
            {
                q2 = new QuestionModel { Text = "Skulle du rekommendera oss?", CreatedByUserId = "admin@edusense.com", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q2);
            }

            var q3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag får bra information om vad som händer på skolan");
            if (q3 is null)
            {
                q3 = new QuestionModel { Text = "Jag får bra information om vad som händer på skolan", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q3);
            }


            var q4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Inomhusmiljön är stimulerande och trivsam");
            if (q4 is null)
            {
                q4 = new QuestionModel { Text = "Inomhusmiljön är stimulerande och trivsam", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q4);
            }

            var q5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Utomhusmiljön är stimulerande för mitt barn");
            if (q5 is null)
            {
                q5 = new QuestionModel { Text = "Utomhusmiljön är stimulerande för mitt barn", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q5);
            }

            var q6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att mitt barn äter skolmaten");
            if (q6 is null)
            {
                q6 = new QuestionModel { Text = "Jag upplever att mitt barn äter skolmaten", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q6);
            }

            var q7 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Fritidshemmet erbjuder en utvecklande verksamhet för mitt barn");
            if (q7 is null)
            {
                q7 = new QuestionModel { Text = "Fritidshemmet erbjuder en utvecklande verksamhet för mitt barn", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q7);
            }

            var q8 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att mitt barn trivs på fritids");
            if (q8 is null)
            {
                q8 = new QuestionModel { Text = "Jag upplever att mitt barn trivs på fritids", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q8);
            }

            var q9 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att skolan arbetar aktivt mot diskriminering och trakasserier");
            if (q9 is null)
            {
                q9 = new QuestionModel { Text = "Jag upplever att skolan arbetar aktivt mot diskriminering och trakasserier", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q9);
            }

            var q10 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Mitt barn får den arbetsro hen behöver");
            if (q10 is null)
            {
                q10 = new QuestionModel { Text = "Mitt barn får den arbetsro hen behöver", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q10);
            }

            var q11 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag känner mig trygg med personalens tillsyn över eleverna");
            if (q11 is null)
            {
                q11 = new QuestionModel { Text = "Jag känner mig trygg med personalens tillsyn över eleverna", CreatedByUserId = "admin@edusense.se", CreatedAt = QuestionBankCreatedAt };
                context.Questions.Add(q11);
            }

            // Medarbetarenkät - frågor
            var qMed1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag upplever att jag har en rimlig arbetsbelastning");
            if (qMed1 is null)
            {
                qMed1 = new QuestionModel { Text = "Jag upplever att jag har en rimlig arbetsbelastning", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qMed1);
            }

            var qMed2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Min närmaste chef ger mig det stöd jag behöver");
            if (qMed2 is null)
            {
                qMed2 = new QuestionModel { Text = "Min närmaste chef ger mig det stöd jag behöver", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qMed2);
            }

            var qMed3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag får möjlighet till kompetensutveckling i min roll");
            if (qMed3 is null)
            {
                qMed3 = new QuestionModel { Text = "Jag får möjlighet till kompetensutveckling i min roll", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLarande.Id };
                context.Questions.Add(qMed3);
            }

            var qMed4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag känner mig delaktig i beslut som rör min arbetsplats");
            if (qMed4 is null)
            {
                qMed4 = new QuestionModel { Text = "Jag känner mig delaktig i beslut som rör min arbetsplats", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qMed4);
            }

            var qMed5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Samarbetet mellan kollegor fungerar bra");
            if (qMed5 is null)
            {
                qMed5 = new QuestionModel { Text = "Samarbetet mellan kollegor fungerar bra", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qMed5);
            }

            var qMed6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag trivs med min fysiska arbetsmiljö");
            if (qMed6 is null)
            {
                qMed6 = new QuestionModel { Text = "Jag trivs med min fysiska arbetsmiljö", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLokaler.Id };
                context.Questions.Add(qMed6);
            }

            var qMed7 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag skulle rekommendera denna arbetsplats till en kollega");
            if (qMed7 is null)
            {
                qMed7 = new QuestionModel { Text = "Jag skulle rekommendera denna arbetsplats till en kollega", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qMed7);
            }

            // Elevenkät åk 7-9 - frågor
            var q79_1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag får den studiero jag behöver på lektionerna");
            if (q79_1 is null)
            {
                q79_1 = new QuestionModel { Text = "Jag får den studiero jag behöver på lektionerna", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catUndervisning.Id };
                context.Questions.Add(q79_1);
            }

            var q79_2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Undervisningen känns varierad och intressant");
            if (q79_2 is null)
            {
                q79_2 = new QuestionModel { Text = "Undervisningen känns varierad och intressant", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catUndervisning.Id };
                context.Questions.Add(q79_2);
            }

            var q79_3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag vågar be om hjälp när jag inte förstår");
            if (q79_3 is null)
            {
                q79_3 = new QuestionModel { Text = "Jag vågar be om hjälp när jag inte förstår", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(q79_3);
            }

            var q79_4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag känner mig trygg i skolmiljön");
            if (q79_4 is null)
            {
                q79_4 = new QuestionModel { Text = "Jag känner mig trygg i skolmiljön", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(q79_4);
            }

            var q79_5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag är delaktig i hur mitt lärande planeras");
            if (q79_5 is null)
            {
                q79_5 = new QuestionModel { Text = "Jag är delaktig i hur mitt lärande planeras", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLarande.Id };
                context.Questions.Add(q79_5);
            }

            var q79_6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Skolans lokaler fungerar bra för mitt lärande");
            if (q79_6 is null)
            {
                q79_6 = new QuestionModel { Text = "Skolans lokaler fungerar bra för mitt lärande", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLokaler.Id };
                context.Questions.Add(q79_6);
            }

            var q79_7 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag trivs på min skola");
            if (q79_7 is null)
            {
                q79_7 = new QuestionModel { Text = "Jag trivs på min skola", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(q79_7);
            }

            // Elevenkät gymnasiet - frågor
            var qGym1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Undervisningen förbereder mig väl för fortsatta studier eller arbete");
            if (qGym1 is null)
            {
                qGym1 = new QuestionModel { Text = "Undervisningen förbereder mig väl för fortsatta studier eller arbete", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catUndervisning.Id };
                context.Questions.Add(qGym1);
            }

            var qGym2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag får den studievägledning jag behöver");
            if (qGym2 is null)
            {
                qGym2 = new QuestionModel { Text = "Jag får den studievägledning jag behöver", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qGym2);
            }

            var qGym3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag har möjlighet att påverka innehållet i mina kurser");
            if (qGym3 is null)
            {
                qGym3 = new QuestionModel { Text = "Jag har möjlighet att påverka innehållet i mina kurser", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLarande.Id };
                context.Questions.Add(qGym3);
            }

            var qGym4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag känner mig stressad av kraven i skolan");
            if (qGym4 is null)
            {
                qGym4 = new QuestionModel { Text = "Jag känner mig stressad av kraven i skolan", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qGym4);
            }

            var qGym5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Skolans lokaler och utrustning håller god standard");
            if (qGym5 is null)
            {
                qGym5 = new QuestionModel { Text = "Skolans lokaler och utrustning håller god standard", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLokaler.Id };
                context.Questions.Add(qGym5);
            }

            var qGym6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag trivs i mötet med lärare och personal");
            if (qGym6 is null)
            {
                qGym6 = new QuestionModel { Text = "Jag trivs i mötet med lärare och personal", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qGym6);
            }

            var qGym7 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag är nöjd med mina studieval hittills");
            if (qGym7 is null)
            {
                qGym7 = new QuestionModel { Text = "Jag är nöjd med mina studieval hittills", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLarande.Id };
                context.Questions.Add(qGym7);
            }

            // Enkät vuxenutbildning - frågor
            var qVux1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Utbildningen går att kombinera med mitt övriga liv (arbete/familj)");
            if (qVux1 is null)
            {
                qVux1 = new QuestionModel { Text = "Utbildningen går att kombinera med mitt övriga liv (arbete/familj)", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qVux1);
            }

            var qVux2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Undervisningen är anpassad efter mina förkunskaper");
            if (qVux2 is null)
            {
                qVux2 = new QuestionModel { Text = "Undervisningen är anpassad efter mina förkunskaper", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catUndervisning.Id };
                context.Questions.Add(qVux2);
            }

            var qVux3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag får det stöd jag behöver från lärare och studievägledare");
            if (qVux3 is null)
            {
                qVux3 = new QuestionModel { Text = "Jag får det stöd jag behöver från lärare och studievägledare", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qVux3);
            }

            var qVux4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Utbildningen ger mig relevanta kunskaper för arbetslivet");
            if (qVux4 is null)
            {
                qVux4 = new QuestionModel { Text = "Utbildningen ger mig relevanta kunskaper för arbetslivet", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLarande.Id };
                context.Questions.Add(qVux4);
            }

            var qVux5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag är nöjd med de digitala verktyg som används i utbildningen");
            if (qVux5 is null)
            {
                qVux5 = new QuestionModel { Text = "Jag är nöjd med de digitala verktyg som används i utbildningen", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catUndervisning.Id };
                context.Questions.Add(qVux5);
            }

            var qVux6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Studiemiljön (lokaler/digital plattform) fungerar bra för mig");
            if (qVux6 is null)
            {
                qVux6 = new QuestionModel { Text = "Studiemiljön (lokaler/digital plattform) fungerar bra för mig", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLokaler.Id };
                context.Questions.Add(qVux6);
            }

            var qVux7 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag skulle rekommendera denna utbildning till andra");
            if (qVux7 is null)
            {
                qVux7 = new QuestionModel { Text = "Jag skulle rekommendera denna utbildning till andra", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qVux7);
            }

            // Uppföljningsenkät efter nystart - frågor
            var qIntro1 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Introduktionen gav mig en bra start");
            if (qIntro1 is null)
            {
                qIntro1 = new QuestionModel { Text = "Introduktionen gav mig en bra start", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qIntro1);
            }

            var qIntro2 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag fick den information jag behövde inför starten");
            if (qIntro2 is null)
            {
                qIntro2 = new QuestionModel { Text = "Jag fick den information jag behövde inför starten", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catOrganisation.Id };
                context.Questions.Add(qIntro2);
            }

            var qIntro3 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Bemötandet från personalen kändes välkomnande");
            if (qIntro3 is null)
            {
                qIntro3 = new QuestionModel { Text = "Bemötandet från personalen kändes välkomnande", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qIntro3);
            }

            var qIntro4 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Mina förväntningar har hittills infriats");
            if (qIntro4 is null)
            {
                qIntro4 = new QuestionModel { Text = "Mina förväntningar har hittills infriats", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLarande.Id };
                context.Questions.Add(qIntro4);
            }

            var qIntro5 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Jag vet vart jag ska vända mig om jag har frågor");
            if (qIntro5 is null)
            {
                qIntro5 = new QuestionModel { Text = "Jag vet vart jag ska vända mig om jag har frågor", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catStod.Id };
                context.Questions.Add(qIntro5);
            }

            var qIntro6 = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Lokalerna kändes välkomnande vid start");
            if (qIntro6 is null)
            {
                qIntro6 = new QuestionModel { Text = "Lokalerna kändes välkomnande vid start", CreatedByUserId = "admin@edusense.se", CreatedAt = ExpandedSurveysCreatedAt, CategoryId = catLokaler.Id };
                context.Questions.Add(qIntro6);
            }

            // Koppla varje fråga till rätt kategori (idempotent - sätts om varje körning, skapar inga dubbletter)
            q1.CategoryId = catOrganisation.Id;
            q2.CategoryId = catOrganisation.Id;
            q3.CategoryId = catOrganisation.Id;
            q4.CategoryId = catLokaler.Id;
            q5.CategoryId = catLokaler.Id;
            q6.CategoryId = catLokaler.Id;
            q7.CategoryId = catLarande.Id;
            q8.CategoryId = catStod.Id;
            q9.CategoryId = catStod.Id;
            q10.CategoryId = catUndervisning.Id;
            q11.CategoryId = catStod.Id;

            // Fixar CreatedAt för frågor som redan seedades innan fältet sattes här (annars
            // visar frågehanteringen 0001-01-01) - rör bara rader som fortfarande har standardvärdet.
            if (q1.CreatedAt == default) q1.CreatedAt = QuestionBankCreatedAt;
            if (q2.CreatedAt == default) q2.CreatedAt = QuestionBankCreatedAt;
            if (q3.CreatedAt == default) q3.CreatedAt = QuestionBankCreatedAt;
            if (q4.CreatedAt == default) q4.CreatedAt = QuestionBankCreatedAt;
            if (q5.CreatedAt == default) q5.CreatedAt = QuestionBankCreatedAt;
            if (q6.CreatedAt == default) q6.CreatedAt = QuestionBankCreatedAt;
            if (q7.CreatedAt == default) q7.CreatedAt = QuestionBankCreatedAt;
            if (q8.CreatedAt == default) q8.CreatedAt = QuestionBankCreatedAt;
            if (q9.CreatedAt == default) q9.CreatedAt = QuestionBankCreatedAt;
            if (q10.CreatedAt == default) q10.CreatedAt = QuestionBankCreatedAt;
            if (q11.CreatedAt == default) q11.CreatedAt = QuestionBankCreatedAt;

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
            // allQuestions används fortsatt för att länka till survey1 (Kundnöjdhetsenkät) nedan -
            // newSurveyQuestions ska bara få svarsalternativ, inte hamna i survey1.
            var allQuestions = new[] { q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11 };
            var newSurveyQuestions = new[]
            {
                qMed1, qMed2, qMed3, qMed4, qMed5, qMed6, qMed7,
                q79_1, q79_2, q79_3, q79_4, q79_5, q79_6, q79_7,
                qGym1, qGym2, qGym3, qGym4, qGym5, qGym6, qGym7,
                qVux1, qVux2, qVux3, qVux4, qVux5, qVux6, qVux7,
                qIntro1, qIntro2, qIntro3, qIntro4, qIntro5, qIntro6
            };
            var allAnswerOptions = new[] { ans1, ans2, ans3, ans4, ans5 };

            foreach (var question in allQuestions.Concat(newSurveyQuestions))
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

            // Geografisk data på organisationerna (Malmö, Lund)
            org1.Latitude = 55.60482333;
            org1.Longitude = 13.0050694;
            org2.Latitude = 55.70389;
            org2.Longitude = 13.19500;
            await context.SaveChangesAsync();

            // NPS-fråga med egen 1-10-skala (delas inte med de övriga frågornas 1-5-svarsalternativ)
            var qNps = await context.Questions.SingleOrDefaultAsync(x => x.Text == "Hur sannolikt är det att du skulle rekommendera oss till en vän eller kollega?");
            if (qNps is null)
            {
                qNps = new QuestionModel
                {
                    Text = "Hur sannolikt är det att du skulle rekommendera oss till en vän eller kollega?",
                    CreatedByUserId = "admin@edusense.com",
                    CreatedAt = NpsQuestionCreatedAt
                };
                context.Questions.Add(qNps);
                await context.SaveChangesAsync();
            }

            var npsAnswerOptions = new List<AnswerOptionModel>();
            for (var value = 1; value <= 10; value++)
            {
                var description = $"NPS: {value}";
                var option = await context.AnswerOptions.SingleOrDefaultAsync(x => x.Description == description && x.Value == value);
                if (option is null)
                {
                    option = new AnswerOptionModel { Description = description, Value = value };
                    context.AnswerOptions.Add(option);
                }

                npsAnswerOptions.Add(option);
            }

            await context.SaveChangesAsync();

            foreach (var option in npsAnswerOptions)
            {
                if (!await context.QuestionAnswerOptions.AnyAsync(x => x.QuestionId == qNps.Id && x.AnswerOptionId == option.Id))
                {
                    context.QuestionAnswerOptions.Add(new QuestionAnswerOptionModel { QuestionId = qNps.Id, AnswerOptionId = option.Id });
                }
            }

            await context.SaveChangesAsync();

            // Enkät 1 - alla 11 frågor + NPS
            var survey1 = await EnsureSurveyAsync(context, "Kundnöjdhetsenkät", org1.Id);
            await LinkQuestionsToSurveyAsync(context, survey1, allQuestions);
            await LinkQuestionsToSurveyAsync(context, survey1, new[] { qNps });
            var dispatch1 = await EnsureDispatchAsync(context, survey1, DateTime.UtcNow.AddDays(30), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatch1, "respondent1@test.com", "token-123", RespondentSegment.Personal);
            await EnsureRespondentAsync(context, dispatch1, "respondent2@test.com", "token-456", RespondentSegment.Personal);

            // Enkät 2 - föräldraenkät, skol-/fritidsrelaterade frågor
            var survey2 = await EnsureSurveyAsync(context, "Föräldraenkät - skola och fritids", org1.Id);
            await LinkQuestionsToSurveyAsync(context, survey2, [q3, q4, q5, q6, q7, q8, q9, q10, q11]);
            var dispatch2 = await EnsureDispatchAsync(context, survey2, DateTime.UtcNow.AddDays(14), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatch2, "respondent3@test.com", "token-789", RespondentSegment.GradeFTo6);
            await EnsureRespondentAsync(context, dispatch2, "respondent4@test.com", "token-101", RespondentSegment.GradeFTo6);

            // ---- Omfattande svars- och respondentdata för analys/trend ----
            var random = new Random(12345); // fast seed - deterministiskt vid omkörning

            var qaoByQuestionId = (await context.QuestionAnswerOptions
                    .Include(x => x.AnswerOption)
                    .ToListAsync())
                .GroupBy(x => x.QuestionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var criticalQuestionIds = new HashSet<int> { q4.Id, q5.Id, q6.Id }; // Lokaler & Miljö - skevas mot sämre snitt
            var npsQuestionIds = new HashSet<int> { qNps.Id };

            var survey1Questions = await context.SurveyQuestions.Where(x => x.SurveyId == survey1.Id).ToListAsync();
            var survey2Questions = await context.SurveyQuestions.Where(x => x.SurveyId == survey2.Id).ToListAsync();

            // Fler respondenter på de befintliga utskicken
            await SeedBulkRespondentsAsync(context, dispatch1, survey1Questions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 130, random);
            await SeedBulkRespondentsAsync(context, dispatch2, survey2Questions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 130, random);

            // Extra historiska utskick av survey1 för en tidslinje (3, 2 och 1 månad tillbaka)
            var dispatch1b = await EnsureAdditionalDispatchAsync(context, survey1, 1, DateTime.UtcNow.AddMonths(-3), DateTime.UtcNow.AddMonths(-3).AddDays(14), "admin@edusense.com");
            // Fast token på ett redan förfallet utskick, så expired-flödet fortfarande går att testa utan
            // en egen "Trivselenkät" bara för det syftet.
            await EnsureRespondentAsync(context, dispatch1b, "respondent5@test.com", "token-expired", RespondentSegment.Grade7To9);
            await SeedBulkRespondentsAsync(context, dispatch1b, survey1Questions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 80, random);

            var dispatch1c = await EnsureAdditionalDispatchAsync(context, survey1, 2, DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow.AddMonths(-2).AddDays(14), "admin@edusense.com");
            await SeedBulkRespondentsAsync(context, dispatch1c, survey1Questions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 80, random);

            var dispatch1d = await EnsureAdditionalDispatchAsync(context, survey1, 3, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(-1).AddDays(14), "admin@edusense.com");
            await SeedBulkRespondentsAsync(context, dispatch1d, survey1Questions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 80, random);

            // ---- Nya standardenkäter per segment + uppföljningsenkät ----
            var surveyMed = await EnsureSurveyAsync(context, "Medarbetarenkät", org1.Id);
            await LinkQuestionsToSurveyAsync(context, surveyMed, new[] { qMed1, qMed2, qMed3, qMed4, qMed5, qMed6, qMed7 });
            var dispatchMed = await EnsureDispatchAsync(context, surveyMed, DateTime.UtcNow.AddDays(21), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatchMed, "respondent-med1@test.com", "token-med-1", RespondentSegment.Personal);
            var surveyMedQuestions = await context.SurveyQuestions.Where(x => x.SurveyId == surveyMed.Id).ToListAsync();
            await SeedBulkRespondentsAsync(context, dispatchMed, surveyMedQuestions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 60, random);

            var survey79 = await EnsureSurveyAsync(context, "Elevenkät åk 7-9", org1.Id);
            await LinkQuestionsToSurveyAsync(context, survey79, new[] { q79_1, q79_2, q79_3, q79_4, q79_5, q79_6, q79_7 });
            var dispatch79 = await EnsureDispatchAsync(context, survey79, DateTime.UtcNow.AddDays(21), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatch79, "respondent-79-1@test.com", "token-79-1", RespondentSegment.Grade7To9);
            var survey79Questions = await context.SurveyQuestions.Where(x => x.SurveyId == survey79.Id).ToListAsync();
            await SeedBulkRespondentsAsync(context, dispatch79, survey79Questions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 90, random);

            var surveyGym = await EnsureSurveyAsync(context, "Elevenkät gymnasiet", org1.Id);
            await LinkQuestionsToSurveyAsync(context, surveyGym, new[] { qGym1, qGym2, qGym3, qGym4, qGym5, qGym6, qGym7 });
            var dispatchGym = await EnsureDispatchAsync(context, surveyGym, DateTime.UtcNow.AddDays(21), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatchGym, "respondent-gym1@test.com", "token-gym-1", RespondentSegment.Gymnasiet);
            var surveyGymQuestions = await context.SurveyQuestions.Where(x => x.SurveyId == surveyGym.Id).ToListAsync();
            await SeedBulkRespondentsAsync(context, dispatchGym, surveyGymQuestions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 90, random);

            var surveyVux = await EnsureSurveyAsync(context, "Enkät vuxenutbildning", org1.Id);
            await LinkQuestionsToSurveyAsync(context, surveyVux, new[] { qVux1, qVux2, qVux3, qVux4, qVux5, qVux6, qVux7 });
            var dispatchVux = await EnsureDispatchAsync(context, surveyVux, DateTime.UtcNow.AddDays(21), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatchVux, "respondent-vux1@test.com", "token-vux-1", RespondentSegment.Vuxenutbildning);
            var surveyVuxQuestions = await context.SurveyQuestions.Where(x => x.SurveyId == surveyVux.Id).ToListAsync();
            await SeedBulkRespondentsAsync(context, dispatchVux, surveyVuxQuestions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 70, random);

            var surveyIntro = await EnsureSurveyAsync(context, "Uppföljningsenkät efter nystart", org1.Id);
            await LinkQuestionsToSurveyAsync(context, surveyIntro, new[] { qIntro1, qIntro2, qIntro3, qIntro4, qIntro5, qIntro6 });
            var dispatchIntro = await EnsureDispatchAsync(context, surveyIntro, DateTime.UtcNow.AddDays(21), "admin@edusense.com");
            await EnsureRespondentAsync(context, dispatchIntro, "respondent-intro1@test.com", "token-intro-1", RespondentSegment.GradeFTo6);
            var surveyIntroQuestions = await context.SurveyQuestions.Where(x => x.SurveyId == surveyIntro.Id).ToListAsync();
            await SeedBulkRespondentsAsync(context, dispatchIntro, surveyIntroQuestions, qaoByQuestionId, criticalQuestionIds, npsQuestionIds, 60, random);
        }



        private static async Task<SurveyModel> EnsureSurveyAsync(EduSenseDbContext context, string title, int organisationId)
        {
            var survey = await context.Surveys.SingleOrDefaultAsync(x => x.Title == title && x.OrganisationId == organisationId);
            if (survey is null)
            {
                survey = new SurveyModel
                {
                    Title = title,
                    CreatedByUserId = "admin@edusense.com",
                    OrganisationId = organisationId
                };

                context.Surveys.Add(survey);
                await context.SaveChangesAsync();
            }

            return survey;
        }
        private static async Task<SurveyDispatchModel> EnsureDispatchAsync(
            EduSenseDbContext context, SurveyModel survey, DateTime responseDeadline, string sentByUserId)
        {
            // OrderBy + First istället för Single: en survey kan numera ha flera dispatches
            // (se EnsureAdditionalDispatchAsync) - detta är alltid det första/ursprungliga.
            var dispatch = await context.SurveyDispatches
                .Where(x => x.SurveyId == survey.Id)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (dispatch is null)
            {
                dispatch = new SurveyDispatchModel
                {
                    SurveyId = survey.Id,
                    ResponseDeadline = responseDeadline,
                    SentByUserId = sentByUserId,
                    SentAt = DateTime.UtcNow
                };

                context.SurveyDispatches.Add(dispatch);
                await context.SaveChangesAsync();
            }

            return dispatch;
        }

        // Idempotent hantering av "ytterligare" utskick för en survey som redan har ett vanligt
        // utskick (skapat av EnsureDispatchAsync). Ordinal = 0-baserat index i utskicksordningen
        // (ordinal 0 är alltid det vanliga utskicket) - så omkörning hittar rätt befintligt utskick
        // istället för att skapa fler, utan att behöva ett eget urskiljande fält på modellen.
        private static async Task<SurveyDispatchModel> EnsureAdditionalDispatchAsync(
            EduSenseDbContext context, SurveyModel survey, int ordinal, DateTime sentAt, DateTime responseDeadline, string sentByUserId)
        {
            var existingDispatches = await context.SurveyDispatches
                .Where(x => x.SurveyId == survey.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (existingDispatches.Count > ordinal)
                return existingDispatches[ordinal];

            var dispatch = new SurveyDispatchModel
            {
                SurveyId = survey.Id,
                SentAt = sentAt,
                ResponseDeadline = responseDeadline,
                SentByUserId = sentByUserId
            };

            context.SurveyDispatches.Add(dispatch);
            await context.SaveChangesAsync();

            return dispatch;
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

        private static async Task<RespondentModel> EnsureRespondentAsync(
            EduSenseDbContext context, SurveyDispatchModel surveyDispatch, string email, string token, RespondentSegment segment)
        {
            var respondent = await context.Respondents
                .SingleOrDefaultAsync(x => x.Email == email && x.SurveyDispatchId == surveyDispatch.Id);

            if (respondent is null)
            {
                respondent = new RespondentModel
                {
                    Email = email,
                    Token = token,
                    SurveyDispatchId = surveyDispatch.Id,
                    Segment = segment,
                    TokenIsUsed = false
                };

                context.Respondents.Add(respondent);
            }
            else
            {
                respondent.Segment = segment; // idempotent - sätts om varje körning
            }

            await context.SaveChangesAsync();

            return respondent;
        }

        private static readonly RespondentSegment[] AllSegments =
        [
            RespondentSegment.GradeFTo6,
            RespondentSegment.Grade7To9,
            RespondentSegment.Gymnasiet,
            RespondentSegment.Vuxenutbildning,
            RespondentSegment.Personal
        ];

        // Skevning mot "Nöjd"/"Mycket nöjd" för vanliga 1-5-frågor
        private static readonly (int Value, int Weight)[] SatisfiedScaleWeights =
        [
            (5, 35), (4, 35), (3, 15), (2, 10), (1, 5)
        ];

        // Klart sämre snitt för de "kritiska" frågorna (Lokaler & Miljö) - ger ett tydligt kritiskt område i analysen
        private static readonly (int Value, int Weight)[] CriticalScaleWeights =
        [
            (5, 8), (4, 17), (3, 25), (2, 28), (1, 22)
        ];

        // NPS 1-10, skevad mot promoters (9-10) med ett tydligt men inte överdrivet svansat gäng detractors (1-6)
        private static readonly (int Value, int Weight)[] NpsWeights =
        [
            (10, 27), (9, 21), (8, 15), (7, 12), (6, 8),
            (5, 5), (4, 4), (3, 3), (2, 2), (1, 3)
        ];

        // Väger fram ett värde ur en (Value, Weight)-tabell utifrån en delad, seedad Random.
        private static int PickWeightedValue(Random random, (int Value, int Weight)[] weights)
        {
            var total = weights.Sum(w => w.Weight);
            var roll = random.Next(total);
            var cumulative = 0;

            foreach (var (value, weight) in weights)
            {
                cumulative += weight;
                if (roll < cumulative)
                    return value;
            }

            return weights[^1].Value; // ska aldrig nås
        }

        // Seedar ett antal nya respondenter på ett utskick, med spritt Segment och en realistisk
        // svarsfrekvens (~65-75%) - obesvarade respondenter får inga Response-rader.
        private static async Task SeedBulkRespondentsAsync(
            EduSenseDbContext context,
            SurveyDispatchModel dispatch,
            IReadOnlyList<SurveyQuestionModel> surveyQuestions,
            IReadOnlyDictionary<int, List<QuestionAnswerOptionModel>> qaoByQuestionId,
            HashSet<int> criticalQuestionIds,
            HashSet<int> npsQuestionIds,
            int respondentCount,
            Random random)
        {
            const double answeredRatio = 0.70; // ca 65-75% av nya respondenter har svarat

            // En enda rundtur för befintlighetskollen istället för en AnyAsync per respondent.
            var existingEmails = (await context.Respondents
                .Where(x => x.SurveyDispatchId == dispatch.Id)
                .Select(x => x.Email)
                .ToListAsync())
                .ToHashSet();

            var newRespondents = new List<RespondentModel>();

            for (var i = 1; i <= respondentCount; i++)
            {
                var email = $"respondent-d{dispatch.Id}-{i}@test.com";

                if (existingEmails.Contains(email))
                    continue; // redan seedad i en tidigare körning

                var hasAnswered = random.NextDouble() < answeredRatio;
                var segment = AllSegments[(i - 1) % AllSegments.Length]; // sprider segment jämnt över alla fem värden

                newRespondents.Add(new RespondentModel
                {
                    Email = email,
                    Token = $"token-d{dispatch.Id}-{i}",
                    SurveyDispatchId = dispatch.Id,
                    Segment = segment,
                    TokenIsUsed = hasAnswered,
                    TokenUsedAt = hasAnswered
                        ? DateTime.UtcNow.AddDays(-random.Next(1, 60)).AddHours(-random.Next(0, 24))
                        : null
                });
            }

            if (newRespondents.Count == 0)
                return;

            // En enda rundtur för samtliga nya respondenter - EF fyller i alla genererade Id:n på en gång.
            context.Respondents.AddRange(newRespondents);
            await context.SaveChangesAsync();

            var newResponses = new List<ResponseModel>();

            foreach (var respondent in newRespondents)
            {
                if (!respondent.TokenIsUsed)
                    continue;

                foreach (var surveyQuestion in surveyQuestions)
                {
                    var options = qaoByQuestionId[surveyQuestion.QuestionId];
                    var isCritical = criticalQuestionIds.Contains(surveyQuestion.QuestionId);
                    var isNps = npsQuestionIds.Contains(surveyQuestion.QuestionId);

                    var weights = isNps ? NpsWeights : isCritical ? CriticalScaleWeights : SatisfiedScaleWeights;
                    var chosenValue = PickWeightedValue(random, weights);
                    var chosenOption = options.Single(x => x.AnswerOption!.Value == chosenValue);

                    newResponses.Add(new ResponseModel
                    {
                        RespondentId = respondent.Id,
                        SurveyQuestionId = surveyQuestion.Id,
                        QuestionAnswerOptionId = chosenOption.Id
                    });
                }
            }

            // En enda rundtur för samtliga svar, istället för en per respondent.
            context.Responses.AddRange(newResponses);
            await context.SaveChangesAsync();
        }
    }
}
