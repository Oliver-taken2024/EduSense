using Bunit;
using Bunit.TestDoubles;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    /// <summary>
    /// Fejkad HttpMessageHandler för ForgotPasswordPage-tester
    /// som kan ge olika svar beroende på request.
    /// </summary>
    public class ForgotPasswordFakeHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public ForgotPasswordFakeHttpMessageHandler(
            Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responder(request));
        }
    }

    public class ForgotPasswordPageTest : TestContext
    {
        private ForgotPasswordFakeHttpMessageHandler _httpHandler = null!;
        private FakeNavigationManager _navigationManager = null!;

        private void RegisterServices(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _httpHandler = new ForgotPasswordFakeHttpMessageHandler(responder);
            var httpClient = new HttpClient(_httpHandler)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            Services.AddSingleton(httpClient);
            Services.AddSingleton<ApiService>();

            // Get FakeNavigationManager after registering services
            _navigationManager = Services.GetRequiredService<FakeNavigationManager>();
        }

        [Fact]
        public void ForgotPasswordPage_Should_Render_Successfully()
        {
            // Arrange
            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var component = RenderComponent<ForgotPasswordPage>();

            // Assert
            Assert.NotNull(component);
            Assert.Contains("ForgotPassword", component.Markup);
        }

        [Fact]
        public void ForgotPasswordPage_Should_Render_Email_Input()
        {
            // Arrange
            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Assert
            Assert.NotNull(emailInput);
            Assert.Equal("Enter your email", emailInput.GetAttribute("placeholder"));
        }

        [Fact]
        public void ForgotPasswordPage_Should_Render_Submit_Button()
        {
            // Arrange
            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var component = RenderComponent<ForgotPasswordPage>();
            var button = component.Find("button");

            // Assert
            Assert.NotNull(button);
            Assert.Contains("Skicka återställning", button.TextContent);
        }

        [Fact]
        public void ForgotPasswordPage_Email_Input_Binding()
        {
            // Arrange
            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change("test@example.com");

            // Assert
            Assert.Equal("test@example.com", emailInput.GetAttribute("value"));
        }

        [Fact]
        public void ForgotPasswordPage_SendReset_Makes_API_Call()
        {
            // Arrange
            var testEmail = "user@example.com";
            RegisterServices(request =>
            {
                if (request.Method == HttpMethod.Post && 
                    request.RequestUri?.AbsolutePath.Contains("forgot-password") == true)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("mockToken123")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);
            var button = component.Find("button");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                Assert.Single(_httpHandler.Requests);
                var request = _httpHandler.Requests[0];
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Contains("forgot-password", request.RequestUri?.AbsolutePath ?? "");
            });
        }

        [Fact]
        public void ForgotPasswordPage_On_Success_Navigates_To_Reset_Password()
        {
            // Arrange
            var testEmail = "user@example.com";
            var testToken = "resetToken123";

            RegisterServices(request =>
            {
                if (request.Method == HttpMethod.Post && 
                    request.RequestUri?.AbsolutePath.Contains("forgot-password") == true)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(testToken)
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);
            var button = component.Find("button");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                var expectedUrl = $"/reset-password?email={testEmail}&token={Uri.EscapeDataString(testToken)}";
                Assert.EndsWith(expectedUrl, _navigationManager.Uri);
            });
        }

        [Fact]
        public void ForgotPasswordPage_On_Failure_Does_Not_Navigate()
        {
            // Arrange
            var testEmail = "user@example.com";

            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var component = RenderComponent<ForgotPasswordPage>();
            var initialUri = _navigationManager.Uri;
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);
            var button = component.Find("button");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                // Uri should not change after failed request
                Assert.Equal(initialUri, _navigationManager.Uri);
            });
        }

        [Fact]
        public void ForgotPasswordPage_Empty_Email_Still_Makes_Request()
        {
            // Arrange
            RegisterServices(request =>
            {
                if (request.Method == HttpMethod.Post && 
                    request.RequestUri?.AbsolutePath.Contains("forgot-password") == true)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("token")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var component = RenderComponent<ForgotPasswordPage>();
            var button = component.Find("button");

            // Act
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                Assert.Single(_httpHandler.Requests);
            });
        }

        [Fact]
        public void ForgotPasswordPage_Multiple_Submit_Attempts()
        {
            // Arrange
            var testEmail = "user@example.com";
            var requestCount = 0;

            RegisterServices(request =>
            {
                if (request.Method == HttpMethod.Post && 
                    request.RequestUri?.AbsolutePath.Contains("forgot-password") == true)
                {
                    requestCount++;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent($"token{requestCount}")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");
            var button = component.Find("button");

            // Act - First submission
            emailInput.Change(testEmail);
            button.Click();

            component.WaitForAssertion(() =>
            {
                Assert.Single(_httpHandler.Requests);
            });

            // Change email and submit again
            emailInput.Change("another@example.com");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                Assert.Equal(2, _httpHandler.Requests.Count);
            });
        }

        [Fact]
        public void ForgotPasswordPage_Preserves_Email_Value()
        {
            // Arrange
            var testEmail = "preserved@example.com";
            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);

            // Assert - Value is preserved in input
            Assert.Equal(testEmail, emailInput.GetAttribute("value"));
        }

        [Fact]
        public void ForgotPasswordPage_Handles_Server_Error()
        {
            // Arrange
            var testEmail = "user@example.com";

            RegisterServices(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var component = RenderComponent<ForgotPasswordPage>();
            var initialUri = _navigationManager.Uri;
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);
            var button = component.Find("button");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                Assert.Single(_httpHandler.Requests);
                // Navigation should not occur on error
                Assert.Equal(initialUri, _navigationManager.Uri);
            });
        }

        [Fact]
        public void ForgotPasswordPage_Request_Contains_Correct_Endpoint()
        {
            // Arrange
            var testEmail = "verify@example.com";
            RegisterServices(request =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("token")
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);
            var button = component.Find("button");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                Assert.Single(_httpHandler.Requests);
                var request = _httpHandler.Requests[0];
                Assert.Contains("api/auth/forgot-password", request.RequestUri?.AbsolutePath ?? "");
            });
        }

        [Fact]
        public void ForgotPasswordPage_Navigation_With_Escaped_Token()
        {
            // Arrange
            var testEmail = "user@example.com";
            var testToken = "token/with/special+chars";
            var expectedEncodedToken = Uri.EscapeDataString(testToken);

            RegisterServices(request =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(testToken)
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });

            var component = RenderComponent<ForgotPasswordPage>();
            var emailInput = component.Find("input[type=\"email\"]");

            // Act
            emailInput.Change(testEmail);
            var button = component.Find("button");
            button.Click();

            // Assert
            component.WaitForAssertion(() =>
            {
                var expectedUrl = $"/reset-password?email={testEmail}&token={expectedEncodedToken}";
                Assert.EndsWith(expectedUrl, _navigationManager.Uri);
            });
        }
    }
}
