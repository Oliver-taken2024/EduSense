using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduSense.BLL.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduSense.Infrastructure.Ai
{

    // Optioner för OllamaClient som kan konfigureras via appsettings.json eller andra konfigurationskällor.
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "llama3";
    }

    // Implementation av IOllamaClient som använder HttpClient för att kommunicera med Ollama API.
    public class OllamaClient : IOllamaClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;
        private readonly ILogger<OllamaClient> _logger;

        public OllamaClient(HttpClient httpClient, IOptions<OllamaOptions> options, ILogger<OllamaClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (_httpClient.BaseAddress is null)
            {
                _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            }
        }

        // Metod för att generera AI-sammanfattning baserat på systemprompt och användarinnehåll.
        public async Task<string> GenerateAsync(string systemPrompt, string userContent, CancellationToken cancellationToken = default)
        {
            var requestBody = new OllamaGenerateRequest
            {
                Model = _options.Model,
                System = systemPrompt,
                Prompt = userContent,
                Stream = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/generate", requestBody, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);
                return result?.Response?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid anrop till Ollama.");
                throw new InvalidOperationException("Kunde inte generera AI-sammanfattning just nu.", ex);
            }
        }

        // Interna klasser för att representera request och response för Ollama API.
        private class OllamaGenerateRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("system")]
            public string System { get; set; } = string.Empty;

            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }
        }

        private class OllamaGenerateResponse
        {
            [JsonPropertyName("response")]
            public string? Response { get; set; }
        }
    }
}