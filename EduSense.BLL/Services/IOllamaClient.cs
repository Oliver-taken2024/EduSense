namespace EduSense.BLL.Services
{
    // Interface som definierar kontraktet för en Llama-klient som används för att generera
    // textbaserade svar baserat på systemprompt och användarinnehåll.
    public interface IOllamaClient
    {
        Task<string> GenerateAsync(string systemPrompt, string userContent, CancellationToken cancellationToken = default);
    }
}