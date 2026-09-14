using Bunit;
using EduSense.Shared;
using EduSense.UI.Pages;
using EduSense.UI.Services;
using EduSense.UI.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test;

public class QuestionManagementPageTests : TestContext
{
    private static List<QuestionDto> OneQuestion() =>
    [
        new QuestionDto { Id = 1, Text = "Hur trivs du?" }
    ];

    // Hjälpmetod för att registrera en ApiService med en FakeHttpMessageHandler
    // som returnerar det angivna statuskoden och innehållet.
    private void RegisterApiService(HttpStatusCode statusCode, object? content = null)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(new ApiService(httpClient));
    }

    [Fact]
    //sidan laddar och visar tomt-meddelande om inga frågor finns
    public void Shows_loading_state_before_questions_are_loaded()
    {
        RegisterApiService(HttpStatusCode.OK, new List<QuestionDto>());

        var cut = RenderComponent<QuestionManagementPage>();

        // Laddningstexten ska ha visats vid start, oavsett om den redan hunnit
        // försvinna när vi kommer hit (asynkron OnInitializedAsync).
        cut.WaitForAssertion(() => Assert.Contains("Inga frågor hittades.", cut.Markup));
    }

    [Fact]
    //"Skapa fråga"-knappen syns efter laddning.
    public void Shows_create_button_after_questions_are_loaded()
    {
        RegisterApiService(HttpStatusCode.OK, OneQuestion());

        var cut = RenderComponent<QuestionManagementPage>();

        cut.WaitForAssertion(() => Assert.Contains("Skapa fråga", cut.Markup));
    }

    [Fact]
    //Klick på knappen anropar ShowCreateForm() och visar QuestionForm
    //med titeln "Ny fråga", samtidigt som knappen försvinner.
    public void Clicking_create_button_shows_question_form_with_create_title()
    {
        RegisterApiService(HttpStatusCode.OK, OneQuestion());

        var cut = RenderComponent<QuestionManagementPage>();
        cut.WaitForAssertion(() => Assert.Contains("Skapa fråga", cut.Markup));

        cut.Find("button.button-login").Click();

        Assert.Contains("Ny fråga", cut.Markup);
        Assert.DoesNotContain("Skapa fråga", cut.Markup);
    }

    [Fact]

    //Klick på "Redigera" i QuestionList triggar EditQuestion och visar formuläret förifyllt.
    public void Clicking_edit_shows_question_form_with_edit_title_and_prefilled_text()
    {
        RegisterApiService(HttpStatusCode.OK, OneQuestion());

        var cut = RenderComponent<QuestionManagementPage>();
        cut.WaitForAssertion(() => Assert.Contains("Hur trivs du?", cut.Markup));

        cut.Find("button.button-login[aria-label='Redigera frågan Hur trivs du?']").Click();

        Assert.Contains("Redigera fråga", cut.Markup);
        Assert.Equal("Hur trivs du?", cut.Find("input.form-control").GetAttribute("value"));
    }

    [Fact]
    //döljer formuläret och visar listan igen.
    public void Cancelling_form_returns_to_list_view()
    {
        RegisterApiService(HttpStatusCode.OK, OneQuestion());

        var cut = RenderComponent<QuestionManagementPage>();
        cut.WaitForAssertion(() => Assert.Contains("Skapa fråga", cut.Markup));

        cut.Find("button.button-login").Click();
        Assert.Contains("Ny fråga", cut.Markup);

        cut.Find("button.btn-secondary").Click();

        Assert.Contains("Hur trivs du?", cut.Markup);
        Assert.DoesNotContain("Ny fråga", cut.Markup);
    }

    [Fact]
    //Klick på "Ta bort" öppnar ConfirmDialog med rätt titel/meddelande.
    public void Requesting_delete_shows_confirm_dialog()
    {
        RegisterApiService(HttpStatusCode.OK, OneQuestion());
        JSInterop.SetupVoid("eduSenseFocusTrap.trap", _ => true);

        var cut = RenderComponent<QuestionManagementPage>();
        cut.WaitForAssertion(() => Assert.Contains("Hur trivs du?", cut.Markup));

        cut.Find("button.button-logout[aria-label='Ta bort frågan Hur trivs du?']").Click();

        Assert.Contains("Ta bort fråga", cut.Markup);
        Assert.Contains("Är du säker på att du vill ta bort frågan?", cut.Markup);
    }

    [Fact]
    //Fel vid inläsning visas som felmeddelande.
    public void Load_error_from_api_is_shown()
    {
        RegisterApiService(HttpStatusCode.InternalServerError, new List<string> { "Kunde inte hämta frågor." });

        var cut = RenderComponent<QuestionManagementPage>();

        cut.WaitForAssertion(() => Assert.Contains("Kunde inte hämta frågor.", cut.Markup));
    }
}