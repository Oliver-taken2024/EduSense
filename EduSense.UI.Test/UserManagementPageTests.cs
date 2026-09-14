using Bunit;
using EduSense.Shared;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using EduSense.UI.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    
    // Fejkad HttpMessageHandler som kan ge olika svar beroende på request,
    // via en uppslagsfunktion. För att styra ApiService 
    // utan att göra riktiga nätverksanrop.

    public class RoutingFakeHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        // Metod som simulerar ett HTTP-anrop och returnerar ett svar baserat på request
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(responder(request));
        }
    }

    public class UserManagementPageTests : TestContext
    {
        // Skapar upp två användare, en aktiv och en pending, för att testa olika scenarion i UserManagementPage.
        private static List<UserListItemDto> TwoUsers() =>
        [
            new UserListItemDto
            {
                Id = "1",
                Email = "admin@edusense.com",
                DisplayName = "Admin User",
                IsActive = true,
                EmailConfirmed = true,
                Roles = ["Admin"]
            },
            new UserListItemDto
            {
                Id = "2",
                Email = "pending@edusense.com",
                DisplayName = null,
                IsActive = false,
                EmailConfirmed = false,
                Roles = ["Analyst"]
            }
        ];
        
        // Hjälpmetod för att registrera en ApiService med en RoutingFakeHttpMessageHandler
        // som kan ge olika svar beroende på request.
        private RoutingFakeHttpMessageHandler RegisterApiService(
            Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            var handler = new RoutingFakeHttpMessageHandler(responder);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            Services.AddSingleton(httpClient);
            Services.AddSingleton<ApiService>();

            return handler;
        }
        // Hjälpmetod för att skapa ett HttpResponseMessage med JSON-innehåll och status OK.
        private static HttpResponseMessage JsonOk(object content) =>
            new(HttpStatusCode.OK) { Content = JsonContent.Create(content) };

        // Hjälpmetod för att skapa ett HttpResponseMessage med textinnehåll och status OK.
        private static HttpResponseMessage PlainOk(string text = "OK") =>
            new(HttpStatusCode.OK) { Content = new StringContent(text) };

        [Fact]
        //Sidan hämtar och visar användarlistan från GET api/users.
        public void Shows_loading_users_from_api()
        {
            RegisterApiService(request => JsonOk(TwoUsers()));

            var cut = RenderComponent<UserManagementPage>();

            cut.WaitForAssertion(() =>
            {
                Assert.Contains("admin@edusense.com", cut.Markup);
                Assert.Contains("pending@edusense.com", cut.Markup);
            });
        }

        [Fact]

        //Tomt svar visar "Inga användare hittades."
        public void Shows_empty_message_when_no_users()
        {
            RegisterApiService(request => JsonOk(new List<UserListItemDto>()));

            var cut = RenderComponent<UserManagementPage>();

            cut.WaitForAssertion(() => Assert.Contains("Inga användare hittades.", cut.Markup));
        }

        [Fact]

        //Rätt knappar visas beroende på användarens status.
        public void Pending_user_shows_resend_invite_button_and_active_user_shows_deactivate_button()
        {
            RegisterApiService(request => JsonOk(TwoUsers()));

            var cut = RenderComponent<UserManagementPage>();

            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Skicka inbjudan igen", cut.Markup);
                Assert.Contains("Inaktivera", cut.Markup);
            });
        }

        [Fact]

        //Att skicka inbjudningsformuläret anropar POST api/users/invite och visar bekräftelsemeddelande.
        public void Submitting_invite_form_calls_invite_endpoint_and_shows_status_message()
        {
            var handler = RegisterApiService(request =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/api/users/invite"))
                {
                    return PlainOk("Inbjudan skickad.");
                }

                return JsonOk(new List<UserListItemDto>());
            });

            var cut = RenderComponent<UserManagementPage>();
            cut.WaitForAssertion(() => Assert.Contains("Inga användare hittades.", cut.Markup));

            cut.Find("#invite-email").Change("new.user@edusense.com");
            cut.Find("form").Submit();

            cut.WaitForAssertion(() =>
                Assert.Contains("Inbjudan skickad till new.user@edusense.com", cut.Markup));

            Assert.Contains(handler.Requests, r =>
                r.Method == HttpMethod.Post &&
                r.RequestUri!.AbsolutePath.EndsWith("/api/users/invite"));
        }

        [Fact]
        // Klick på "Skicka inbjudan igen" anropar rätt resend-invite-endpoint med rätt användar-ID.
        public void Clicking_resend_invite_calls_correct_endpoint()
        {
            var handler = RegisterApiService(request =>
            {
                if (request.RequestUri!.AbsolutePath.Contains("/resend-invite"))
                {
                    return PlainOk("Inbjudan skickad på nytt.");
                }

                return JsonOk(TwoUsers());
            });

            var cut = RenderComponent<UserManagementPage>();
            cut.WaitForAssertion(() => Assert.Contains("Skicka inbjudan igen", cut.Markup));

            cut.Find("button.btn-outline-secondary").Click();

            cut.WaitForAssertion(() =>
                Assert.Contains("Inbjudan skickad igen till pending@edusense.com", cut.Markup));

            Assert.Contains(handler.Requests, r =>
                r.Method == HttpMethod.Post &&
                r.RequestUri!.AbsolutePath.EndsWith("/api/users/2/resend-invite"));
        }

        [Fact]
        //Klick på "Inaktivera" anropar rätt endpoint och laddar om listan efteråt.
        public void Clicking_deactivate_calls_correct_endpoint_and_reloads_users()
        {
            var callCount = 0;
            var handler = RegisterApiService(request =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/deactivate"))
                {
                    return PlainOk("Användaren är nu inaktiverad.");
                }

                callCount++;
                return JsonOk(TwoUsers());
            });

            var cut = RenderComponent<UserManagementPage>();
            cut.WaitForAssertion(() => Assert.Contains("Inaktivera", cut.Markup));

            cut.Find("button.button-logout").Click();

            cut.WaitForAssertion(() =>
                Assert.Contains("admin@edusense.com är nu inaktiverad.", cut.Markup));

            Assert.Contains(handler.Requests, r =>
                r.Method == HttpMethod.Post &&
                r.RequestUri!.AbsolutePath.EndsWith("/api/users/1/deactivate"));

            // Listan ska laddas om efter lyckad inaktivering (minst ett GET utöver det initiala).
            Assert.True(callCount >= 2);
        }

        [Fact]
        //Fel vid hämtning av användare visar felmeddelandet från backend.
        public void Api_error_on_load_shows_error_message()
        {
            RegisterApiService(request =>
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = JsonContent.Create(new List<string> { "Kunde inte hämta användare." })
                });

            var cut = RenderComponent<UserManagementPage>();

            cut.WaitForAssertion(() =>
                Assert.Contains("Kunde inte hämta användare.", cut.Markup));
        }
    }
}