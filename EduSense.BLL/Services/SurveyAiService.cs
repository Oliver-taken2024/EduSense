using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using System.ComponentModel.DataAnnotations;
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
        Föreslå en kort handlingsplan per fråga med 2 åtgärder.

        SVARSFORMAT (upprepa blocket nedan för alla tre frågor, i tur och ordning):
        HANDLINGSPLAN FÖR LÄGST NÖJDHET

        1. [Frågetext]
        Betyg: [värde]
        Åtgärder:
        - [åtgärd]
        - [åtgärd]

        REGLER:
        - Svara endast på svenska, max 150 ord totalt.
        - Använd endast information från underlaget, hitta inte på värden eller orsaker.
        - Använd inte Markdown, asterisker eller kodblock.
        """,

            AiPromptType.TrendSummaryReport =>
                """
        Du är en analytiker som sammanfattar enkätresultat för skolledning.

        UPPGIFT:
        Sammanfatta de viktigaste trenderna: styrkor, förbättringsområden och
        eventuella skillnader mellan målgrupper.

        SVARSFORMAT:
        TRENDRAPPORT

        SAMMANFATTNING
        Ett kort stycke, högst 60 ord.

        VIKTIGASTE TRENDER
        1. [Trend]
        2. [Trend]
        3. [Trend]

        REKOMMENDATION
        1-2 konkreta rekommendationer.

        REGLER:
        - Svara endast på svenska, max 120 ord totalt.
        - Använd endast information från underlaget.
        - Använd inte Markdown, asterisker eller kodblock.
        """,

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

        // Läser [Display(Name=...)] från RespondentSegment-enumet, så AI-underlaget
        // blir svenskt (annars skickas t.ex. "GradeFTo6" rakt in i prompten).
        private static string SegmentLabel(RespondentSegment segment)
        {
            var member = typeof(RespondentSegment).GetMember(segment.ToString())[0];
            return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? segment.ToString();
        }
    }
}