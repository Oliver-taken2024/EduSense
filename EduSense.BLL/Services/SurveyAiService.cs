using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace EduSense.BLL.Services
{
    public class SurveyAiService : ISurveyAiService
    {
        // DI av ISurveyDispatchRepository och IOllamaClient via konstruktor
        private readonly ISurveyDispatchRepository _dispatchRepository;
        private readonly IOllamaClient _ollamaClient;

        // Tak på antal frågor som skickas med i AI-prompten. Färre tokens i
        // indata ger kortare prefill-tid för modellen - vid en stor enkät är
        // det ändå extremfallen (lägst nöjdhet) som är relevanta att lyfta.
        private const int MaxQuestionsInPrompt = 8;

        // Konstruktor
        public SurveyAiService(ISurveyDispatchRepository dispatchRepository, IOllamaClient ollamaClient)
        {
            _dispatchRepository = dispatchRepository;
            _ollamaClient = ollamaClient;
        }


        // Metod för att generera AI-sammanfattning baserat på en enkätutskick och prompttyp
        public async Task<SurveyAiSummaryResultDto> GenerateSummaryAsync(SurveyAiSummaryRequestDto request, CancellationToken cancellationToken = default)
        {
            var dispatch = await _dispatchRepository.GetByIdWithResultsAsync(request.SurveyDispatchId)
                ?? throw new InvalidOperationException("Dispatch hittades inte.");

            // Sambanden och deras riktning/styrka är redan matematiskt entydiga (Pearson-r).
            // Den lokala modellen har visat sig hitta på egna frågenamn och värden här
            // istället för att återge de riktiga - så den rapporten byggs helt i kod,
            // ingen AI inblandad.
            if (request.PromptType == AiPromptType.CorrelationAnalysis)
            {
                var correlations = ComputeCorrelations(dispatch);
                return new SurveyAiSummaryResultDto
                {
                    PromptType = request.PromptType,
                    SummaryText = FormatCorrelationReport(correlations),
                    GeneratedAt = DateTime.UtcNow
                };
            }

            var aggregatedData = request.PromptType switch
            {
                AiPromptType.LowestSatisfactionActionPlan => BuildAggregatedResultsText(dispatch),
                AiPromptType.TrendSummaryReport => BuildAggregatedResultsText(dispatch),
                _ => throw new ArgumentOutOfRangeException(nameof(request), request.PromptType, "Okänd prompttyp.")
            };

            var systemPrompt = GetSystemPrompt(request.PromptType);

            var response = await _ollamaClient.GenerateAsync(systemPrompt, aggregatedData, cancellationToken);

            return new SurveyAiSummaryResultDto
            {
                PromptType = request.PromptType,
                SummaryText = response,
                GeneratedAt = DateTime.UtcNow
            };
        }

        // Metod för att hämta systemprompt baserat på prompttyp
        private static string GetSystemPrompt(AiPromptType type) => type switch
        {
            AiPromptType.LowestSatisfactionActionPlan =>
                """
        Du är en analytiker som hjälper skolor att tolka enkätresultat.

        UPPGIFT:
        Identifiera de tre frågor som har lägst genomsnittligt betyg.
        Föreslå en konkret handlingsplan för varje fråga med 2–3 korta åtgärder.

        SVARSFORMAT:
        HANDLINGSPLAN FÖR LÄGST NÖJDHET

        1. [Frågetext]
        Genomsnittligt betyg: [värde]
        Åtgärder:
        - [åtgärd]
        - [åtgärd]

        2. [Frågetext]
        Genomsnittligt betyg: [värde]
        Åtgärder:
        - [åtgärd]
        - [åtgärd]

        3. [Frågetext]
        Genomsnittligt betyg: [värde]
        Åtgärder:
        - [åtgärd]
        - [åtgärd]

        REGLER:
        - Svara endast på svenska.
        - Var konkret och kortfattad.
        - Använd endast information från underlaget.
        - Hitta inte på värden eller orsaker.
        - Använd inte Markdown, asterisker eller kodblock.
        """,

            AiPromptType.TrendSummaryReport =>
                """
        Du är en analytiker som sammanfattar enkätresultat för skolledning.

        UPPGIFT:
        Sammanfatta de viktigaste trenderna i resultatet.
        Lyft fram positiva resultat, förbättringsområden och eventuella skillnader
        mellan målgrupper.

        SVARSFORMAT:
        TRENDRAPPORT

        SAMMANFATTNING
        Skriv ett kort sammanhängande stycke på högst 100 ord.

        VIKTIGASTE TRENDER
        1. [Trend]
        2. [Trend]
        3. [Trend]

        SKILLNADER MELLAN MÅLGRUPPER
        Beskriv endast tydliga skillnader som framgår av underlaget.

        REKOMMENDATION
        Skriv 2–3 konkreta rekommendationer.

        REGLER:
        - Svara endast på svenska.
        - Max 200 ord totalt.
        - Använd endast information från underlaget.
        - Använd inte Markdown, asterisker eller kodblock.
        """,

            // CorrelationAnalysis går inte via AI-modellen - se ComputeCorrelations/
            // FormatCorrelationReport i GenerateSummaryAsync.

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };


        // Aggregerar per fråga: medelvärde, antal svar, min/max, samt per målgrupp (segment).

        private static string BuildAggregatedResultsText(SurveyDispatchModel dispatch)
        {
            var allResponses = dispatch.Respondents
                .SelectMany(r => r.Responses.Select(resp => new
                {
                    QuestionText = resp.SurveyQuestion!.Question!.Text,
                    resp.QuestionAnswerOption!.AnswerOption!.Value,
                    Segment = r.Segment
                }))
                .ToList();

            if (allResponses.Count == 0)
            {
                return $"Enkät: {dispatch.Survey?.Title}\nInga svar har registrerats för detta enkätutskick ännu.";
            }

            var perQuestion = allResponses
                .GroupBy(r => r.QuestionText)
                .Select(g => new
                {
                    Question = g.Key,
                    Average = g.Average(x => x.Value),
                    Count = g.Count(),
                    Min = g.Min(x => x.Value),
                    Max = g.Max(x => x.Value)
                })
                .OrderBy(q => q.Average)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"Enkät: {dispatch.Survey?.Title}");
            sb.AppendLine($"Antal respondenter som svarat: {dispatch.Respondents.Count(r => r.TokenIsUsed)} av {dispatch.Respondents.Count}");
            sb.AppendLine();

            // Begränsar till de MaxQuestionsInPrompt frågorna med lägst betyg - kortare
            // indata ger kortare prefill-tid, och det är ändå ytterligheterna som är
            // relevanta för handlingsplan/trendanalys. perQuestion är redan sorterad
            // lägst-först (OrderBy Average ovan).
            var questionsToInclude = perQuestion.Take(MaxQuestionsInPrompt).ToList();
            var wasTruncated = perQuestion.Count > questionsToInclude.Count;

            sb.AppendLine(wasTruncated
                ? $"Resultat per fråga (de {MaxQuestionsInPrompt} med lägst nöjdhet av {perQuestion.Count} totalt):"
                : "Resultat per fråga (medelvärde på svarsskalan, lägst nöjdhet först):");

            foreach (var q in questionsToInclude)
            {
                sb.AppendLine($"- \"{q.Question}\": medel {q.Average:F1} (min {q.Min}, max {q.Max}, {q.Count} svar)");
            }

            var perSegment = allResponses
                .GroupBy(r => r.Segment)
                .Select(g => new { Segment = g.Key, Average = g.Average(x => x.Value), Count = g.Count() });

            sb.AppendLine();
            sb.AppendLine("Resultat per målgrupp:");
            foreach (var s in perSegment)
            {
                sb.AppendLine($"- {SegmentLabel(s.Segment)}: medel {s.Average:F1} ({s.Count} svar)");
            }

            return sb.ToString();
        }

   
        // Beräknar Pearson-korrelation (se Wikipedia el dyl) mellan frågor baserat på per-respondent-svar,
        // och returnerar endast starka samband (|r| >= 0.5). Ingen PII inkluderas.
        private static List<(string A, string B, double Correlation)> ComputeCorrelations(SurveyDispatchModel dispatch)
        {
            var respondentAnswers = dispatch.Respondents
                .Where(r => r.TokenIsUsed)
                .Select(r => r.Responses.ToDictionary(
                    resp => resp.SurveyQuestion!.Question!.Text,
                    resp => (double)resp.QuestionAnswerOption!.AnswerOption!.Value))
                .ToList();

            var questions = respondentAnswers.SelectMany(r => r.Keys).Distinct().ToList();
            var correlations = new List<(string A, string B, double Correlation)>();

            for (int i = 0; i < questions.Count; i++)
            {
                for (int j = i + 1; j < questions.Count; j++)
                {
                    var pairs = respondentAnswers
                        .Where(r => r.ContainsKey(questions[i]) && r.ContainsKey(questions[j]))
                        .Select(r => (X: r[questions[i]], Y: r[questions[j]]))
                        .ToList();

                    if (pairs.Count < 3) continue;

                    var corr = PearsonCorrelation(pairs);
                    if (Math.Abs(corr) >= 0.5)
                    {
                        correlations.Add((questions[i], questions[j], corr));
                    }
                }
            }

            return correlations;
        }

        // Formaterar sambandsanalysen direkt från de beräknade värdena - ingen AI
        // inblandad, så frågenamn och korrelationsvärden kan aldrig bli fel.
        private static string FormatCorrelationReport(List<(string A, string B, double Correlation)> correlations)
        {
            if (correlations.Count == 0)
            {
                return "SAMBANDSANALYS\n\nInga starka statistiska samband (korrelationsvärde 0,5 eller högre) hittades mellan frågorna i detta underlag.\n\nSLUTSATS\nDet gick inte att identifiera några tydliga samband. Fler svar kan behövas för en tillförlitlig analys.";
            }

            var top = correlations.OrderByDescending(c => Math.Abs(c.Correlation)).Take(3).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("SAMBANDSANALYS");
            sb.AppendLine();

            for (int i = 0; i < top.Count; i++)
            {
                var c = top[i];
                sb.AppendLine($"{i + 1}. \"{c.A}\" och \"{c.B}\"");
                sb.AppendLine($"Korrelationsvärde: {c.Correlation.ToString("F2", CultureInfo.GetCultureInfo("sv-SE"))}");
                sb.AppendLine($"Tolkning: {DescribeCorrelation(c.Correlation)}");
                sb.AppendLine();
            }

            sb.AppendLine("SLUTSATS");
            sb.Append(top.Count == 1
                ? "Ett tydligt samband hittades mellan frågorna ovan."
                : $"{top.Count} tydliga samband hittades mellan frågorna ovan.");

            return sb.ToString();
        }

        // Riktning och styrka är en ren funktion av korrelationsvärdet - ingen tolkning
        // som kan hittas på.
        private static string DescribeCorrelation(double r)
        {
            var styrka = Math.Abs(r) >= 0.7 ? "starkt" : "medelstarkt";
            return r >= 0
                ? $"Det finns ett {styrka} positivt samband: högre svar på den ena frågan hänger ihop med högre svar på den andra."
                : $"Det finns ett {styrka} negativt samband: högre svar på den ena frågan hänger ihop med lägre svar på den andra.";
        }

        private static double PearsonCorrelation(List<(double X, double Y)> pairs)
        {
            var avgX = pairs.Average(p => p.X);
            var avgY = pairs.Average(p => p.Y);

            var numerator = pairs.Sum(p => (p.X - avgX) * (p.Y - avgY));
            var denomX = Math.Sqrt(pairs.Sum(p => Math.Pow(p.X - avgX, 2)));
            var denomY = Math.Sqrt(pairs.Sum(p => Math.Pow(p.Y - avgY, 2)));

            return denomX == 0 || denomY == 0 ? 0 : numerator / (denomX * denomY);
        }

        // Läser [Display(Name=...)] från RespondentSegment-enumet, så AI-underlaget
        // blir svenskt (annars skickas t.ex. "GradeFTo6" rakt in i prompten).
        private static string SegmentLabel(RespondentSegment segment)
        {
            var member = typeof(RespondentSegment).GetMember(segment.ToString())[0];
            return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? segment.ToString();
        }
    }
}