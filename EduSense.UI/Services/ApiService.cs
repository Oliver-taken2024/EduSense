using System.Net.Http.Json;

namespace EduSense.UI.Services
{
    public class ApiService(HttpClient httpClient)
    {
        // Metoder för att göra HTTP-anrop till backend-API:et.
        public async Task<T?> GetAsync<T>(string endpoint)
        { 
            var response = await httpClient.GetAsync(endpoint);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T?> PostAsync<T>(string endpoint, object payload)
        {
            var response = await httpClient.PostAsJsonAsync(endpoint, payload);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        // Vissa endpoints svarar med vanlig text istället för JSON. Den här metoden används
        // när svaret inte behöver läsas, så slipper vi krascha vid tolkning av det.
        public async Task PostAsync(string endpoint, object payload)
        {
            var response = await httpClient.PostAsJsonAsync(endpoint, payload);
            await EnsureSuccessAsync(response);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object payload)
        {
            var response = await httpClient.PutAsJsonAsync(endpoint, payload);
            await EnsureSuccessAsync(response);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await httpClient.DeleteAsync(endpoint);
            await EnsureSuccessAsync(response);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            List<string>? errors = null;
            try
            {
                // Vår backend svarar med en JSON-array av felmeddelanden vid 400 (se SurveyController).
                errors = await response.Content.ReadFromJsonAsync<List<string>>();
            }
            catch
            {
                // Body var inte en JSON-array av strängar (t.ex. tomt 404-svar) — faller tillbaka nedan.
            }

            errors ??= [response.ReasonPhrase ?? "Okänt fel"];

            throw new ApiException((int)response.StatusCode, errors);
        }
    }
}
