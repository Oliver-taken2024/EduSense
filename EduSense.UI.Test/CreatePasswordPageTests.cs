using Bunit;
using Bunit.TestDoubles;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using EduSense.UI.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System;

using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{

    // Fejkad HttpMessageHandler som ger ett fast svar tillbaka utan att
    // göra ett riktigt nätverksanrop, så att ApiService kan testas utan en
    // riktig backend.
    
    public class FakeHttpMessageHandler(HttpStatusCode statusCode, object? content = null) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        // Metod som simulerar ett HTTP-anrop och returnerar ett fast svar
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            var response = new HttpResponseMessage(statusCode)
            {
                Content = content is null
                    ? new StringContent(string.Empty)
                    : JsonContent.Create(content)
            };

            return Task.FromResult(response);
        }
    }

    public class CreatePasswordPageTests : TestContext
    {
        private void RegisterApiService(HttpMessageHandler handler)
        {
            // Skapar en HttpClient med den fejkade handlern och registrerar ApiService

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            Services.AddSingleton(httpClient);
            Services.AddSingleton<ApiService>();
        }

       
        // Navigerar till sidan med angivna query-parametrar, eftersom
        // [SupplyParameterFromQuery] bara kan fyllas via en riktig navigering
        // och inte via RenderComponent-parametrar.

        private void NavigateToPage(string? userId, string? token)
        {
            var navigationManager = Services.GetRequiredService<FakeNavigationManager>();

            var query = new List<string>();
            if (userId is not null)
            {
                query.Add($"userId={Uri.EscapeDataString(userId)}");
            }
            if (token is not null)
            {
                query.Add($"token={Uri.EscapeDataString(token)}");
            }

            var uri = "skapa-losenord";
            if (query.Count > 0)
            {
                uri += "?" + string.Join("&", query);
            }

            navigationManager.NavigateTo(uri);
        }

        [Fact]
        public void Missing_query_parameters_shows_error_message()
        {
            // Testar att sidan visar ett felmeddelande när userId och token saknas i query-parametrarna

            RegisterApiService(new FakeHttpMessageHandler(HttpStatusCode.OK));
            NavigateToPage(userId: null, token: null);

            var cut = RenderComponent<CreatePasswordPage>();

            Assert.Contains("saknar nödvändig information", cut.Markup);
        }

        [Fact]
        public void Renders_form_when_userId_and_token_are_present()
        {
            // Testar att formuläret renderas när userId och token finns i query-parametrarna

            RegisterApiService(new FakeHttpMessageHandler(HttpStatusCode.OK));
            NavigateToPage("user-1", "token-1");

            var cut = RenderComponent<CreatePasswordPage>();

            Assert.Contains("Nytt lösenord", cut.Markup);
            Assert.Contains("Bekräfta lösenord", cut.Markup);
        }

        [Fact]
        public void Mismatched_passwords_shows_validation_error_and_does_not_call_api()
        {
            // Testar att om lösenorden inte matchar, visas ett valideringsfel och API-anropet görs inte

            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
            RegisterApiService(handler);
            NavigateToPage("user-1", "token-1");

            var cut = RenderComponent<CreatePasswordPage>();

            cut.Find("#new-password").Change("Password123!");
            cut.Find("#confirm-password").Change("SomethingElse123!");
            cut.Find("form").Submit();

            Assert.Contains("matchar inte", cut.Markup);
            Assert.Null(handler.LastRequest);
        }

        [Fact]
        public void Successful_submit_shows_success_message_and_redirects_to_login()
        {
            // Testar att vid lyckad lösenordsskapande, visas ett meddelande
            // och användaren omdirigeras till inloggningssidan

            RegisterApiService(new FakeHttpMessageHandler(HttpStatusCode.OK, new { }));
            NavigateToPage("user-1", "token-1");

            var cut = RenderComponent<CreatePasswordPage>();

            cut.Find("#new-password").Change("Password123!");
            cut.Find("#confirm-password").Change("Password123!");
            cut.Find("form").Submit();

            cut.WaitForAssertion(
                () => Assert.Contains("Lösenordet är skapat", cut.Markup),
                timeout: TimeSpan.FromSeconds(5));

            var navigationManager = Services.GetRequiredService<FakeNavigationManager>();

            cut.WaitForAssertion(
                () => Assert.EndsWith("/login", navigationManager.Uri),
                timeout: TimeSpan.FromSeconds(6));
        }

        [Fact]
        public void Api_error_shows_error_message_from_backend()
        {
            // Testar att om API-anropet returnerar ett fel, visas felmeddelandet från backend

            var handler = new FakeHttpMessageHandler(
                HttpStatusCode.BadRequest,
                new List<string> { "Token har gått ut." });

            RegisterApiService(handler);
            NavigateToPage("user-1", "token-1");

            var cut = RenderComponent<CreatePasswordPage>();

            cut.Find("#new-password").Change("Password123!");
            cut.Find("#confirm-password").Change("Password123!");
            cut.Find("form").Submit();

            cut.WaitForAssertion(
                () => Assert.Contains("Token har gått ut.", cut.Markup),
                timeout: TimeSpan.FromSeconds(5));
        }
    }
}