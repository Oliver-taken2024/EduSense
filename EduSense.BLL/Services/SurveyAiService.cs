using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using System.Text;

namespace EduSense.BLL.Services
{
    public class SurveyAiService : ISurveyAiService
    {
        // DI av ISurveyDispatchRepository och IOllamaClient via konstruktor
        private readonly ISurveyDispatchRepository _dispatchRepository;
        private readonly IOllamaClient _ollamaClient;

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

            var aggregatedData = request.PromptType switch
            {
                AiPromptType.LowestSatisfactionActionPlan => BuildAggregatedResultsText(dispatch),
                AiPromptType.TrendSummaryReport => BuildAggregatedResultsText(dispatch),
                AiPromptType.CorrelationAnalysis => BuildCorrelationInput(dispatch),
                _ => throw new ArgumentOutOfRangeException(nameof(request.PromptType))
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
                "Du är en analytiker som hjälper skolor tolka enkätresultat. " +
                "Identifiera de tre frågor med lägst genomsnittligt betyg i datan nedan och föreslå " +
                "en konkret, kort handlingsplan (2-3 punkter) per fråga. Svara på svenska, koncist.",
            AiPromptType.TrendSummaryReport =>
                "Du är en analytiker. Sammanfatta de viktigaste trenderna i enkätresultaten nedan " +
                "i en kort rapport (max 200 ord) på svenska, riktad till skolledning.",
            AiPromptType.CorrelationAnalysis =>
                "Du är en analytiker. Undersök om det finns tydliga samband mellan hur respondenter " +
                "svarat på olika frågor (t.ex. högt på en fråga, lågt på en annan). Beskriv max 3 samband " +
                "kort och konkret på svenska.",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

    
        // Aggregerar per fråga: medelvärde, antal svar, min/max, samt per målgrupp (segment).
      
        private static string BuildAggregatedResultsText(SurveyDispatchModel dispatch)
        {
            var allResponses = dispatch.Respondents
                .SelectMany(r => r.Responses.Select(resp => new
                {
                    QuestionText = resp.SurveyQuestion!.Question!.Text,
                    Value = resp.QuestionAnswerOption!.AnswerOption!.Value,
                    Segment = r.Segment
                }))
                .ToList();

            if (allResponses.Count == 0)
            {
                return "Inga svar har registrerats för detta enkätutskick ännu.";
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
            sb.AppendLine("Resultat per fråga (medelvärde på svarsskalan, lägst nöjdhet först):");

            foreach (var q in perQuestion)
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
                sb.AppendLine($"- {s.Segment}: medel {s.Average:F1} ({s.Count} svar)");
            }

            return sb.ToString();
        }

   
        // Beräknar Pearson-korrelation (se Wikipedia el dyl) mellan frågor baserat på per-respondent-svar,
        // och returnerar endast starka samband (|r| >= 0.5). Ingen PII inkluderas.
      
        private static string BuildCorrelationInput(SurveyDispatchModel dispatch)
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

            var sb = new StringBuilder();
            sb.AppendLine("Statistiskt beräknade samband mellan frågor (Pearson-korrelation, |r| >= 0.5):");
            foreach (var c in correlations.OrderByDescending(c => Math.Abs(c.Correlation)))
            {
                sb.AppendLine($"- \"{c.A}\" och \"{c.B}\": r = {c.Correlation:F2}");
            }

            return correlations.Count == 0
                ? "Inga starka statistiska samband hittades mellan frågorna."
                : sb.ToString();
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
    }
}