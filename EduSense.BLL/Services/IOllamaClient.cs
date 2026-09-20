namespace EduSense.BLL.Services
{
    // Interface som definierar kontraktet för en Llama-klient som används för att generera
    // textbaserade svar baserat på systemprompt och användarinnehåll.
    public interface IOllamaClient
    {
        Task<string> GenerateAsync(string systemPrompt, string userContent, CancellationToken cancellationToken = default);
    }

    // Egen exception-typ för Ollama-fel (timeout, nätverksfel, ej nåbar) - skild från
    // InvalidOperationException så att SurveyAiController kan särskilja "dispatch
    // hittades inte" (404) från "AI-tjänsten svarade inte" (ska inte visas som 404).
    public class OllamaUnavailableException : Exception
    {
        public OllamaUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}