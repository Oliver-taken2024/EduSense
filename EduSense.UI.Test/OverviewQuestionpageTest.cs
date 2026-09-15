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
    public class OverviewQuestionPageTest : TestContext
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

        private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, object? content = null) =>
            new(statusCode)
            {
                Content = content is null ? new StringContent(string.Empty) : JsonContent.Create(content)
            };

        private static RespondentSurveyDto MakeSurvey(int questionCount = 2, int answeredQuestionIndex = -1)
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
                        new RespondentAnswerOptionDto { Id = i * 10 + 2, Description = "Neutral", Value = 3 },
                        new RespondentAnswerOptionDto { Id = i * 10 + 3, Description = "Instämmer inte alls", Value = 1 }
                    ],
                    AnsweredQuestionAnswerOptionId = i == answeredQuestionIndex + 1 ? i * 10 + 1 : null
                });
            }

            return new RespondentSurveyDto
            {
                Title = "Testenkät",
                Description = "En testbeskrivning",
                IsCompleted = false,
                Questions = questions
            };
        }

        [Fact]
        public void Page_loads_survey_and_displays_questions()
        {
            var survey = MakeSurvey(questionCount: 2);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Granska dina svar", cut.Markup);
                Assert.Contains("Fråga text 1", cut.Markup);
                Assert.Contains("Fråga text 2", cut.Markup);
            });
        }

        [Fact]
        public void All_answer_options_are_displayed_for_each_question()
        {
            var survey = MakeSurvey(questionCount: 1);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                Assert.Contains("Instämmer helt", cut.Markup);
                Assert.Contains("Neutral", cut.Markup);
                Assert.Contains("Instämmer inte alls", cut.Markup);
            });
        }

        [Fact]
        public void Previously_selected_answer_is_checked()
        {
            var survey = MakeSurvey(questionCount: 2, answeredQuestionIndex: 0); // First question answered with option 11
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                var radioButtons = cut.FindAll("input[type=\"radio\"]");
                var firstQuestionAnswerButton = radioButtons.FirstOrDefault(rb => rb.GetAttribute("value") == "11");

                Assert.NotNull(firstQuestionAnswerButton);
                Assert.True(firstQuestionAnswerButton!.GetAttribute("checked") == "True" || 
                           firstQuestionAnswerButton.HasAttribute("checked"));
            });
        }

        [Fact]
        public void Selecting_answer_updates_the_answer()
        {
            var survey = MakeSurvey(questionCount: 1);
            var responses = new List<HttpRequestMessage>();
            var handler = new RoutingFakeHttpMessageHandler(req =>
            {
                responses.Add(req);
                if (req.RequestUri?.AbsolutePath.Contains("/api/respondent") == true)
                {
                    return JsonResponse(HttpStatusCode.OK, survey);
                }
                return JsonResponse(HttpStatusCode.OK, new { success = true });
            });
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() => Assert.Contains("Fråga text 1", cut.Markup));

            var firstRadioButton = cut.Find("input[type=\"radio\"][value=\"11\"]");
            firstRadioButton.Change(true);

            cut.WaitForAssertion(() =>
            {
                var saveRequest = responses.FirstOrDefault(r =>
                    r.RequestUri?.AbsolutePath.Contains("/api/result") == true &&
                    r.Method == HttpMethod.Post);
                Assert.NotNull(saveRequest);
            });
        }

        [Fact]
        public void Submit_button_is_displayed()
        {
            var survey = MakeSurvey(questionCount: 1);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                var submitButton = cut.Find("button");
                Assert.Contains("Skicka in enkät", submitButton.TextContent);
            });
        }

        [Fact]
        public void Submit_button_sends_complete_request_and_navigates()
        {
            var survey = MakeSurvey(questionCount: 1);
            var responses = new List<HttpRequestMessage>();
            var handler = new RoutingFakeHttpMessageHandler(req =>
            {
                responses.Add(req);
                if (req.RequestUri?.AbsolutePath.Contains("/api/respondent") == true)
                {
                    return JsonResponse(HttpStatusCode.OK, survey);
                }
                return JsonResponse(HttpStatusCode.OK, new { success = true });
            });
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() => Assert.Contains("Skicka in enkät", cut.Markup));

            var submitButton = cut.Find("button");
            submitButton.Click();  

            cut.WaitForAssertion(() =>
            {
                var completeRequest = responses.FirstOrDefault(r =>
                    r.RequestUri?.AbsolutePath.Contains("/api/result/token-1/complete") == true &&
                    r.Method == HttpMethod.Post);
                Assert.NotNull(completeRequest);
            });
        }

        [Fact]
        public void Survey_with_multiple_questions_displays_all()
        {
            var survey = MakeSurvey(questionCount: 5);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    Assert.Contains($"Fråga text {i}", cut.Markup);
                }
            });
        }

        [Fact]
        public void Fieldset_and_legend_are_properly_structured_for_accessibility()
        {
            var survey = MakeSurvey(questionCount: 1);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                var fieldsets = cut.FindAll("fieldset");
                Assert.NotEmpty(fieldsets);

                var legends = cut.FindAll("legend");
                Assert.NotEmpty(legends);
            });
        }

        [Fact]
        public void Main_element_has_correct_id_for_skip_link()
        {
            var survey = MakeSurvey(questionCount: 1);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                var mainElement = cut.Find("main");
                Assert.NotNull(mainElement);
                Assert.Equal("main-content", mainElement.GetAttribute("id"));
            });
        }

        [Fact]
        public void Skip_link_is_present_for_keyboard_navigation()
        {
            var survey = MakeSurvey(questionCount: 1);
            var handler = new RoutingFakeHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, survey));
            RegisterApiService(handler);

            var cut = RenderComponent<OverviewQuestionPage>(p => p.Add(x => x.Token, "token-1"));

            cut.WaitForAssertion(() =>
            {
                var skipLink = cut.Find("a.skip-link");
                Assert.NotNull(skipLink);
                Assert.Equal("#main-content", skipLink.GetAttribute("href"));
            });
        }
    }
}

