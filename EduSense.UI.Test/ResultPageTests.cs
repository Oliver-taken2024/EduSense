using Bunit;
using EduSense.Shared;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class ResultPageTests : TestContext
    {
        // Till skillnad från FakeHttpMessageHandler (som alltid svarar likadant) routar den här
        // på URL:ens path-prefix, eftersom ResultPage anropar tre olika endpoints.
        private sealed class RoutingHandler(Dictionary<string, object> responsesByPathPrefix) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var path = request.RequestUri!.AbsolutePath.TrimStart('/');
                var match = responsesByPathPrefix.First(kv => path.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase));

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(match.Value)
                };
                return Task.FromResult(response);
            }
        }

        private void RegisterApiService(Dictionary<string, object> responsesByPathPrefix)
        {
            var handler = new RoutingHandler(responsesByPathPrefix);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            Services.AddSingleton(new ApiService(httpClient));
        }

        [Fact]
        public void Renders_dispatch_list_on_load()
        {
            var dispatches = new List<SurveyDispatchDto>
            {
                new() { Id = 1, SurveyTitle = "Kundnöjdhetsenkät", ResponseCount = 5, RespondentCount = 10 }
            };

            RegisterApiService(new Dictionary<string, object>
            {
                ["api/surveydispatch"] = dispatches
            });

            var cut = RenderComponent<ResultPage>();

            cut.WaitForAssertion(() => Assert.Contains("Kundnöjdhetsenkät", cut.Markup));
        }

        [Fact]
        public void Shows_placeholder_when_no_dispatches_exist()
        {
            RegisterApiService(new Dictionary<string, object>
            {
                ["api/surveydispatch"] = new List<SurveyDispatchDto>()
            });

            var cut = RenderComponent<ResultPage>();

            cut.WaitForAssertion(() => Assert.Contains("Inga utskick hittades.", cut.Markup));
        }

        [Fact]
        public void Clicking_ShowResult_LoadsAndDisplaysDispatchResult()
        {
            var dispatches = new List<SurveyDispatchDto>
            {
                new() { Id = 1, SurveyTitle = "Kundnöjdhetsenkät", ResponseCount = 5, RespondentCount = 10 }
            };
            var result = new SurveyResultDto { TotalResponses = 5, AverageScore = 4.2 };

            RegisterApiService(new Dictionary<string, object>
            {
                ["api/surveydispatch"] = dispatches,
                ["api/result/dispatch/1"] = result,
                ["api/result/organisations"] = new List<OrganisationLocationDto>()
            });

            var cut = RenderComponent<ResultPage>();
            cut.WaitForAssertion(() => cut.Find("button.button-login"));

            cut.Find("button.button-login").Click();

            cut.WaitForAssertion(() => Assert.Contains("4,2", cut.Markup));
        }

        
        [Fact]
public void ShowsAiUnavailableMessage_WhenNoPromptTypesExist()
{
    var apiService = new Mock<ApiService>();

    apiService
        .Setup(x => x.GetAsync<List<AiPromptTypeOptionDto>>(
            It.IsAny<string>()))
        .ReturnsAsync(new List<AiPromptTypeOptionDto>());

    Services.AddSingleton(apiService.Object);

    var cut = RenderComponent<ResultPage>();

    cut.WaitForAssertion(() =>
    {
        Assert.Contains(
            "AI-assistans är inte tillgänglig",
            cut.Markup);
    });
}

        

    }
}
