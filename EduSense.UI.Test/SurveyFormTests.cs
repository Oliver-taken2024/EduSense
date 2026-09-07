using Bunit;
using EduSense.Shared;
using EduSense.UI.Components;
using EduSense.UI.Services;
using EduSense.UI.Test.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class SurveyFormTests : TestContext
    {
        private void RegisterApiService(HttpStatusCode statusCode, object? content = null)
        {
            var handler = new FakeHttpMessageHandler(statusCode, content);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            Services.AddSingleton(new ApiService(httpClient));
        }

        [Fact]
        public void Shows_create_title_when_no_survey_given()
        {
            RegisterApiService(HttpStatusCode.OK);

            var cut = RenderComponent<SurveyForm>();

            Assert.Contains("Ny enkät", cut.Markup);
        }

        [Fact]
        public void Shows_edit_title_and_prefills_fields_when_editing()
        {
            RegisterApiService(HttpStatusCode.OK);
            var survey = new SurveyDto
            {
                Id = 7,
                Title = "Befintlig enkät",
                OrganisationId = 2,
                SurveyQuestions = [new SurveyQuestionDto { Id = 1, QuestionId = 3, QuestionText = "Fråga?" }]
            };

            var cut = RenderComponent<SurveyForm>(parameters => parameters
                .Add(p => p.Survey, survey));

            Assert.Contains("Redigera enkät", cut.Markup);
            Assert.Equal("Befintlig enkät", cut.Find("#title").GetAttribute("value"));
        }

        [Fact]
        public async Task Save_new_survey_calls_OnSaved_on_success()
        {
            RegisterApiService(HttpStatusCode.OK, new SurveyDto { Id = 1, Title = "Ny enkät" });
            var saved = false;

            var cut = RenderComponent<SurveyForm>(parameters => parameters
                .Add(p => p.OnSaved, EventCallback.Factory.Create(this, () => saved = true)));

            cut.Find("#title").Input("Ny enkät");
            await cut.Find("form").SubmitAsync();

            Assert.True(saved);
        }

        [Fact]
        public async Task Save_shows_errors_and_does_not_call_OnSaved_on_ApiException()
        {
            RegisterApiService(HttpStatusCode.BadRequest, new List<string> { "Det finns redan en enkät med samma titel, utgångsdatum och organisation." });
            var saved = false;

            var cut = RenderComponent<SurveyForm>(parameters => parameters
                .Add(p => p.OnSaved, EventCallback.Factory.Create(this, () => saved = true)));

            cut.Find("#title").Input("Dubblett");
            await cut.Find("form").SubmitAsync();

            Assert.False(saved);
            Assert.Contains("Det finns redan en enkät med samma titel, utgångsdatum och organisation.", cut.Markup);
        }

        [Fact]
        public async Task Cancel_button_invokes_OnCancelled()
        {
            RegisterApiService(HttpStatusCode.OK);
            var cancelled = false;

            var cut = RenderComponent<SurveyForm>(parameters => parameters
                .Add(p => p.OnCancelled, EventCallback.Factory.Create(this, () => cancelled = true)));

            await cut.Find("button.btn-secondary").ClickAsync(new MouseEventArgs());

            Assert.True(cancelled);
        }

        [Fact]
        public void Prefills_checked_questions_when_editing()
        {
            RegisterApiService(HttpStatusCode.OK);
            var survey = new SurveyDto
            {
                Id = 1,
                Title = "Enkät",
                SurveyQuestions = [new SurveyQuestionDto { Id = 1, QuestionId = 5, QuestionText = "Fråga A" }]
            };
            var questions = new List<QuestionDto>
            {
                new() { Id = 5, Text = "Fråga A" },
                new() { Id = 6, Text = "Fråga B" }
            };

            var cut = RenderComponent<SurveyForm>(parameters => parameters
                .Add(p => p.Survey, survey)
                .Add(p => p.Questions, questions));

            var checkboxes = cut.FindAll("input[type=checkbox]");
            Assert.True(checkboxes[0].HasAttribute("checked"));
            Assert.False(checkboxes[1].HasAttribute("checked"));
        }
    }
}
