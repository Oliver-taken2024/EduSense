using Bunit;
using EduSense.Shared;
using EduSense.UI.Components;
using Microsoft.AspNetCore.Components;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test;

public class QuestionListTests : TestContext
{
    private static List<QuestionDto> TwoQuestions() =>
    [
        new QuestionDto { Id = 1, Text = "Hur trivs du?", Organisation = new OrganisationDto { Id = 1, Name = "Acme AB" } },
        new QuestionDto { Id = 2, Text = "Vad kan förbättras?" }
    ];

    [Fact]
    public void Renders_all_questions_with_organisation_name()
    {
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, TwoQuestions()));

        Assert.Contains("Hur trivs du?", cut.Markup);
        Assert.Contains("Acme AB", cut.Markup);
        Assert.Contains("Vad kan förbättras?", cut.Markup);
    }

    [Fact]
    public void Shows_empty_message_when_no_questions()
    {
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, new List<QuestionDto>()));

        Assert.Contains("Inga frågor hittades.", cut.Markup);
    }

    [Fact]
    public void Filter_hides_non_matching_questions()
    {
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, TwoQuestions()));

        var searchInput = cut.Find("input[placeholder='Sök fråga...']");
        searchInput.Input("trivs");

        Assert.Contains("Hur trivs du?", cut.Markup);
        Assert.DoesNotContain("Vad kan förbättras?", cut.Markup);
    }

    [Fact]
    public void CreateNew_button_invokes_OnCreateNew()
    {
        var wasCalled = false;
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, new List<QuestionDto>())
            .Add(p => p.OnCreateNew, () => wasCalled = true));

        cut.Find("button.btn-primary").Click();

        Assert.True(wasCalled);
    }

    [Fact]
    public void Edit_button_invokes_OnEdit_with_correct_question()
    {
        QuestionDto? edited = null;
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, TwoQuestions())
            .Add(p => p.OnEdit, EventCallback.Factory.Create<QuestionDto>(this, q => edited = q)));

        cut.FindAll("button.btn-outline-secondary")[0].Click();

        Assert.Equal(1, edited?.Id);
    }

    [Fact]
    public void Copy_button_invokes_OnCopy_with_correct_question()
    {
        QuestionDto? copied = null;
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, TwoQuestions())
            .Add(p => p.OnCopy, EventCallback.Factory.Create<QuestionDto>(this, q => copied = q)));

        cut.FindAll("button.btn-outline-primary")[0].Click();

        Assert.Equal("Hur trivs du?", copied?.Text);
    }

    [Fact]
    public void Delete_button_invokes_OnDelete_with_correct_id()
    {
        int? deletedId = null;
        var cut = RenderComponent<QuestionList>(parameters => parameters
            .Add(p => p.Questions, TwoQuestions())
            .Add(p => p.OnDelete, EventCallback.Factory.Create<int>(this, id => deletedId = id)));

        cut.FindAll("button.btn-outline-danger")[1].Click();

        Assert.Equal(2, deletedId);
    }
}