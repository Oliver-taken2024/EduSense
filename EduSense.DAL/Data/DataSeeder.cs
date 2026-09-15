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
        // Fast tidpunkt för de ursprungliga 11 frågorna i frågebanken, så CreatedAt
        // blir deterministiskt vid omkörning istället för DateTime.UtcNow varje gång.
        private static readonly DateTime QuestionBankCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // NPS-frågan lades till senare än ursprungsfrågebanken, egen tidpunkt av samma anledning.
        private static readonly DateTime NpsQuestionCreatedAt = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

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
