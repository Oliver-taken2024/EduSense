using Bunit;
using EduSense.Shared;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using EduSense.UI.Test.Helpers;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class SurveySendingPageTests : TestContext
    {
        private static readonly List<SurveyDto> DefaultSurveys =
        [
            new() { Id = 1, Title = "Enkät A" },
            new() { Id = 2, Title = "Enkät B" }
        ];

        // Registrerar en fejkad ApiService som svarar med angivna surveys
        // för GET och angivna status/content för POST.
        private void RegisterApiService(
            List<SurveyDto>? surveys = null,
            HttpStatusCode postStatusCode = HttpStatusCode.OK,
            object? postContent = null)
        {
            var handler = new RoutingFakeHttpMessageHandler(request =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath.Contains("api/survey"))
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = JsonContent.Create(surveys ?? DefaultSurveys);
                    return response;
                }

                if (request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath.Contains("api/surveydispatch"))
                {
                    var response = new HttpResponseMessage(postStatusCode);
                    if (postContent is not null)
                    {
                        response.Content = JsonContent.Create(postContent);
                    }
                    return response;
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            Services.AddSingleton(new ApiService(httpClient));
        }

        // Sidan har flera knappar med klassen button-secondary, så vi väljer
        // rätt knapp baserat på text istället för att förlita oss på ordning.
        private static void ClickAddRow(IRenderedComponent<SurveySendingPage> cut) =>
            cut.FindAll("button.button-secondary").First(b => b.TextContent.Contains("Lägg till rad")).Click();

        private static void ClickAddFromPaste(IRenderedComponent<SurveySendingPage> cut) =>
            cut.FindAll("button.button-secondary").First(b => b.TextContent.Contains("inklistrad text")).Click();


        [Fact]

        // Kolla att sidan laddar surveys vid initiering
        public void Loads_surveys_on_initialization()
        {
            RegisterApiService();

            var cut = RenderComponent<SurveySendingPage>();

            Assert.Contains("Enkät A", cut.Markup);
            Assert.Contains("Enkät B", cut.Markup);
        }

        [Fact]

        // Kolla att när man väljer en survey så visas respondent-sektionen
        public void Selecting_survey_shows_respondent_section()
        {
            RegisterApiService();
            var cut = RenderComponent<SurveySendingPage>();

            cut.Find("#survey-select").Change("1");

            Assert.NotNull(cut.Find("#deadline"));
            Assert.NotNull(cut.Find("#paste-area"));
        }

        [Fact]

        // Kolla att när man byter survey så rensas listan med emails
        public void Changing_survey_resets_emails()
        {
            RegisterApiService();
            var cut = RenderComponent<SurveySendingPage>();

            cut.Find("#survey-select").Change("1");
            ClickAddRow(cut); // Lägg till rad
            Assert.Single(cut.FindAll("tbody tr"));

            cut.Find("#survey-select").Change("2");

            Assert.Empty(cut.FindAll("tbody tr"));
        }

        [Fact]

        // Kolla att när man klistrar in emails så filtreras ogiltiga och dubbletter bort
        public void ParsePastedEmails_splits_filters_and_removes_duplicates()
        {
            RegisterApiService();
            var cut = RenderComponent<SurveySendingPage>();
            cut.Find("#survey-select").Change("1");

            cut.Find("#paste-area").Change("a@test.se,b@test.se\ninvalid-row\na@test.se");
            ClickAddFromPaste(cut); // Lägg till från inklistrad text

            var values = cut.FindAll("tbody tr td input").Select(i => i.GetAttribute("value")).ToList();

            Assert.Equal(2, values.Count);
            Assert.Contains("a@test.se", values);
            Assert.Contains("b@test.se", values);
        }

        [Fact]

        // Kolla att man kan ta bort en email från listan
        public void RemoveRow_removes_email_from_list()
        {
            RegisterApiService();
            var cut = RenderComponent<SurveySendingPage>();
            cut.Find("#survey-select").Change("1");
            cut.Find("#paste-area").Change("a@test.se");
            ClickAddFromPaste(cut);

            cut.Find("button.button-logout").Click(); // Ta bort

            Assert.Empty(cut.FindAll("tbody tr"));
        }

        [Fact]

        // Kolla att "Skicka"-knappen är inaktiverad när det inte finns några emails
        public void Send_button_disabled_when_no_emails()
        {
            RegisterApiService();
            var cut = RenderComponent<SurveySendingPage>();
            cut.Find("#survey-select").Change("1");

            var sendButton = cut.Find("button.button-login");

            Assert.True(sendButton.HasAttribute("disabled"));
        }

        [Fact]

        // Kolla att "Skicka"-knappen är aktiverad när det finns emails
        public async Task Send_success_shows_status_message_and_clears_emails()
        {
            RegisterApiService(postContent: new SurveyDispatchDto { Id = 1, SurveyId = 1 });
            var cut = RenderComponent<SurveySendingPage>();
            cut.Find("#survey-select").Change("1");
            cut.Find("#paste-area").Change("a@test.se,b@test.se");
            ClickAddFromPaste(cut);

            await cut.Find("button.button-login").ClickAsync(new MouseEventArgs());

            Assert.Contains("Enkäten skickades till 2 respondenter.", cut.Markup);
            Assert.Empty(cut.FindAll("tbody tr"));
        }

        [Fact]

        // Kolla att när API:et returnerar ett felmeddelande så visas det på sidan
        public async Task Send_ApiException_shows_error_message()
        {
            RegisterApiService(
                postStatusCode: HttpStatusCode.BadRequest,
                postContent: new List<string> { "Minst en respondent krävs." });
            var cut = RenderComponent<SurveySendingPage>();
            cut.Find("#survey-select").Change("1");
            cut.Find("#paste-area").Change("a@test.se");
            ClickAddFromPaste(cut);

            await cut.Find("button.button-login").ClickAsync(new MouseEventArgs());

            Assert.Contains("Minst en respondent krävs.", cut.Markup);
        }
    }
}