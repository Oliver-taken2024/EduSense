using EduSense.Shared;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using EduSense.UI.Test.Helpers;
using Moq;
using Bunit;
using Bunit.TestDoubles;
using Xunit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test.Helpers
{
    public class ResetPasswordPageTest : TestContext
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

        [Fact]
        public void Component_Renders_InitialForm()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
            RegisterApiService(handler);

            // Act
            var component = RenderComponent<ResetPasswordPage>();

            // Assert
            Assert.NotNull(component.Find("input[placeholder*='Nytt lösenord']"));
            Assert.Contains("Byt lösenord", component.Find("button").TextContent);
        }

        [Fact]
        public void OnSubmit_PasswordsDoNotMatch_ShowsError()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
            RegisterApiService(handler);

            var component = RenderComponent<ResetPasswordPage>();

            // Act
            component.Find("input[placeholder*='Nytt lösenord']").Change("Password123!");
            component.Find("input[placeholder*='Bekräfta']").Change("Password456!");
            component.Find("button").Click();

            // Assert
            Assert.Contains("Lösenorden matchar inte", component.Markup);
        }

        [Fact]
        public void OnSubmit_ValidPassword_PassesValidation()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
            RegisterApiService(handler);

            var navigationManager = Services.GetRequiredService<FakeNavigationManager>();
            navigationManager.NavigateTo("reset-password?email=admin@edusense.se&token=valid-token-123");

            var component = RenderComponent<ResetPasswordPage>();

            var password = "ValidPassword123!";

            // Act
            component.Find("input[placeholder*='Nytt lösenord']").Change(password);
            component.Find("input[placeholder*='Bekräfta']").Change(password);
            component.Find("button").Click();

            // Assert - verify that no validation error messages appear
            Assert.DoesNotContain("Lösenorden matchar inte", component.Markup);
            Assert.DoesNotContain("minst 8 tecken", component.Markup);
            Assert.DoesNotContain("stor bokstav", component.Markup);
            Assert.DoesNotContain("liten bokstav", component.Markup);
            Assert.DoesNotContain("siffra", component.Markup);
            Assert.DoesNotContain("specialtecken", component.Markup); 
        }

        [Fact]
        public void OnSubmit_PasswordTooShort_ShowsError()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
            RegisterApiService(handler);

            var component = RenderComponent<ResetPasswordPage>();

            // Act
            component.Find("input[placeholder*='Nytt lösenord']").Change("Short1!");
            component.Find("input[placeholder*='Bekräfta']").Change("Short1!");
            component.Find("button").Click();

            // Assert
            Assert.Contains("minst 8 tecken", component.Markup);
        }
    }
}
