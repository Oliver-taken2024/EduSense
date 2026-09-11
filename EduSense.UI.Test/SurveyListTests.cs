using Bunit;
using EduSense.Shared;
using EduSense.UI.Components;
using Microsoft.AspNetCore.Components;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class SurveyListTests : TestContext
    {
        private static List<SurveyDto> TwoSurveys() =>
            [
            new SurveyDto
            {
                Id = 1,
                Title = "Kundnöjdhet",
                Organisation = new OrganisationDto { Id = 1, Name = "Acme AB" },
                SurveyQuestions = [new SurveyQuestionDto { Id = 1, QuestionId = 1, QuestionText = "Fråga?" }]
            },
        new SurveyDto { Id = 2, Title = "Medarbetarenkät" }
        ];


        [Fact]
        public void Renders_all_surveys_with_organisation_name()
        {
            var cut = RenderComponent<SurveyList>(parameters => parameters
                .Add(p => p.Surveys, TwoSurveys()));

            Assert.Contains("Kundnöjdhet", cut.Markup);
            Assert.Contains("Acme AB", cut.Markup);
            Assert.Contains("Medarbetarenkät", cut.Markup);
        }

        [Fact]
        public void Shows_empty_message_when_no_surveys()
        {
            var cut = RenderComponent<SurveyList>(parameters => parameters
                .Add(p => p.Surveys, new List<SurveyDto>()));

            Assert.Contains("Inga enkäter hittades.", cut.Markup);
        }

        //[Fact]
        //public void CreateNew_button_invokes_OnCreateNew()
        //{
        //    var wasCalled = false;
        //    var cut = RenderComponent<SurveyList>(parameters => parameters
        //        .Add(p => p.Surveys, new List<SurveyDto>())
        //        .Add(p => p.OnCreateNew, () => wasCalled = true));

        //    cut.Find("button.btn-primary").Click();

        //    Assert.True(wasCalled);
        //}

        [Fact]
        public void Edit_button_invokes_OnEdit_with_correct_survey()
        {
            SurveyDto? edited = null;
            var cut = RenderComponent<SurveyList>(parameters => parameters
                .Add(p => p.Surveys, TwoSurveys())
                .Add(p => p.OnEdit, EventCallback.Factory.Create<SurveyDto>(this, s => edited = s)));

            cut.FindAll("button.button-login")[0].Click();

            Assert.Equal(1, edited?.Id);
        }

        [Fact]
        public void Delete_button_invokes_OnDelete_with_correct_id()
        {
            int? deletedId = null;
            var cut = RenderComponent<SurveyList>(parameters => parameters
                .Add(p => p.Surveys, TwoSurveys())
                .Add(p => p.OnDelete, EventCallback.Factory.Create<int>(this, id => deletedId = id)));

            cut.FindAll("button.button-logout")[1].Click();

            Assert.Equal(2, deletedId);
        }
    }
}
