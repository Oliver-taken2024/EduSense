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

namespace EduSense.UI.Test;

public class QuestionFormTests : TestContext
{
    private void RegisterApiService(HttpStatusCode statusCode, object? content = null)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        Services.AddSingleton(new ApiService(httpClient));
    }

    [Fact]
    public void Shows_create_title_when_no_question_given()
    {
        RegisterApiService(HttpStatusCode.OK);

        var cut = RenderComponent<QuestionForm>();

        Assert.Contains("Ny fråga", cut.Markup);
    }

    [Fact]
    public void Shows_edit_title_and_prefills_text_when_editing()
    {
        RegisterApiService(HttpStatusCode.OK);
        var question = new QuestionDto { Id = 7, Text = "Befintlig fråga" };

        var cut = RenderComponent<QuestionForm>(parameters => parameters
            .Add(p => p.Question, question));

        Assert.Contains("Redigera fråga", cut.Markup);
        Assert.Equal("Befintlig fråga", cut.Find("input.form-control").GetAttribute("value"));
    }

    [Fact]
    public async Task Save_new_question_calls_OnSaved_on_success()
    {
        RegisterApiService(HttpStatusCode.OK, new QuestionDto { Id = 1, Text = "Ny text" });
        var saved = false;

        var cut = RenderComponent<QuestionForm>(parameters => parameters
            .Add(p => p.OnSaved, EventCallback.Factory.Create(this, () => saved = true)));

        cut.Find("input.form-control").Input("Ny text");
        await cut.Find("form").SubmitAsync();

        Assert.True(saved);
    }

    [Fact]
    public async Task Save_shows_errors_and_does_not_call_OnSaved_on_ApiException()
    {
        RegisterApiService(HttpStatusCode.BadRequest, new List<string> { "Frågetext får inte vara tom." });
        var saved = false;

        var cut = RenderComponent<QuestionForm>(parameters => parameters
            .Add(p => p.OnSaved, EventCallback.Factory.Create(this, () => saved = true)));

        cut.Find("input.form-control").Input("Text");
        await cut.Find("form").SubmitAsync();

        Assert.False(saved);
        Assert.Contains("Frågetext får inte vara tom.", cut.Markup);
    }

    [Fact]
    public async Task Cancel_button_invokes_OnCancelled()
    {
        RegisterApiService(HttpStatusCode.OK);
        var cancelled = false;

        var cut = RenderComponent<QuestionForm>(parameters => parameters
            .Add(p => p.OnCancelled, EventCallback.Factory.Create(this, () => cancelled = true)));

        await cut.Find("button.btn-secondary").ClickAsync(new MouseEventArgs());

        Assert.True(cancelled);
    }
}