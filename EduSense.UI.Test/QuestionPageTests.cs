using Bunit;
using Bunit.TestDoubles;
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
    public class QuestionPageTests : TestContext
    {
        private void RegisterApiService(HttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            Services.AddSingleton(httpClient);
            Services.AddSingleton<ApiService>();
        }

        private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, object? content = null) => new(statusCode)
        {
            Content = content is null ? new StringContent(string.Empty) : JsonContent.Create(content)
        };

        private static RespondentSurveyDto MakeSurvey(bool isCompleted = false, int questionCount = 1)
        {
            var questions = new List<RespondentQuestionDto>();
            for (var i = 1; i <= questionCount; i++)
            {
                questions.Add(new RespondentQuestionDto
                {
                    SurveyQuestionId = i,
                    QuestionText = $"Fråga text {i}",
                    AnswerOptions =
                    [
                        new RespondentAnswerOptionDto { Id = i * 10 + 1, Description = "Instämmer helt", Value = 5 },
                        new RespondentAnswerOptionDto { Id = i * 10 + 2, Description = "Instämmer inte alls", Value = 1 }
                    ]
                });
            }

            return new RespondentSurveyDto
            {
                Title = "Testenkät",
                Description = "En testbeskrivning",
                IsCompleted = isCompleted,
                Questions = questions
            };
        }

        [Fact]
        public void Unknown_token_shows_not_found_message()
        {
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.NotFound));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "bad-token"));

            cut.WaitForAssertion(() => Assert.Contains("ogiltig", cut.Markup));
        }

        [Fact]
        public void Expired_survey_shows_backend_error_message()
        {
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.BadRequest, new[] { "Enkäten har gått ut." }));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "expired-token"));

            cut.WaitForAssertion(() => Assert.Contains("Enkäten har gått ut.", cut.Markup));
        }

        [Fact]
        public void Valid_token_shows_welcome_screen_with_start_button()
        {
            var survey = MakeSurvey();
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Testenkät", cut.Markup);
                Assert.Contains("Starta Enkäten", cut.Markup);
            });
        }

        [Fact]
        public void Already_completed_survey_shows_thank_you_message()
        {
            var survey = MakeSurvey(isCompleted: true);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() => Assert.Contains("redan besvarat", cut.Markup));
        }

        [Fact]
        public void Resuming_with_a_previous_answer_skips_welcome_and_jumps_to_first_unanswered_question()
        {
            var survey = MakeSurvey(questionCount: 3);
            survey.Questions[0].AnsweredQuestionAnswerOptionId = 11; // fråga 1 redan besvarad sen tidigare besök

            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                Assert.DoesNotContain("Starta Enkäten", cut.Markup);
                Assert.Contains("Fråga 2", cut.Markup);
            });
        }

        [Fact]
        public void Start_button_shows_first_question()
        {
            var survey = MakeSurvey(questionCount: 2);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));
            cut.WaitForAssertion(() => Assert.Contains("Starta Enkäten", cut.Markup));

            cut.Find("button.Question-start-button").Click();

            Assert.Contains("Fråga 1", cut.Markup);
            Assert.Contains("Fråga text 1", cut.Markup);
        }

        [Fact]
        public void Next_button_disabled_until_answer_selected()
        {
            var survey = MakeSurvey();
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));
            cut.WaitForAssertion(() => Assert.Contains("Starta Enkäten", cut.Markup));
            cut.Find("button.Question-start-button").Click();

            var nextButton = cut.FindAll("button.Question-nav-arrow")[1];
            Assert.True(nextButton.HasAttribute("disabled"));

            cut.Find("input[type=radio]").Change(true);

            Assert.False(cut.FindAll("button.Question-nav-arrow")[1].HasAttribute("disabled"));
        }

        [Fact]
        public void Selecting_answer_posts_to_result_api()
        {
            var survey = MakeSurvey();
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));
            cut.WaitForAssertion(() => Assert.Contains("Starta Enkäten", cut.Markup));
            cut.Find("button.Question-start-button").Click();

            cut.Find("input[type=radio]").Change(true);

            cut.WaitForAssertion(() =>
                Assert.Contains(handler.Requests, r => r.Method == HttpMethod.Post && r.RequestUri!.AbsolutePath.EndsWith("api/result")));
        }

        [Fact]
        public void Selected_answer_is_kept_when_navigating_back_and_forward()
        {
            var survey = MakeSurvey(questionCount: 2);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));
            cut.WaitForAssertion(() => Assert.Contains("Starta Enkäten", cut.Markup));
            cut.Find("button.Question-start-button").Click();

            cut.Find("input[type=radio]").Change(true);
            cut.WaitForAssertion(() => Assert.False(cut.FindAll("button.Question-nav-arrow")[1].HasAttribute("disabled")));

            cut.FindAll("button.Question-nav-arrow")[1].Click();
            Assert.Contains("Fråga 2", cut.Markup);

            cut.FindAll("button.Question-nav-arrow")[0].Click();
            Assert.Contains("Fråga 1", cut.Markup);

            Assert.True(cut.Find("input[type=radio]").HasAttribute("checked"));
        }

        [Fact]
        public void Answering_last_question_completes_survey_and_navigates_to_complete_page()
        {
            var survey = MakeSurvey(questionCount: 1);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<QuestionPage>(p => p.Add(x => x.Token, "token-1"));
            cut.WaitForAssertion(() => Assert.Contains("Starta Enkäten", cut.Markup));
            cut.Find("button.Question-start-button").Click();

            cut.Find("input[type=radio]").Change(true);
            cut.WaitForAssertion(() => Assert.False(cut.FindAll("button.Question-nav-arrow")[1].HasAttribute("disabled")));

            cut.FindAll("button.Question-nav-arrow")[1].Click();

            var navigationManager = Services.GetRequiredService<FakeNavigationManager>();
            cut.WaitForAssertion(() => Assert.EndsWith("/enkat-klar", navigationManager.Uri), timeout: TimeSpan.FromSeconds(5));
        }
    }
}
