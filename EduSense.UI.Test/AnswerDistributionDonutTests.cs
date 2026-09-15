using Bunit;
using EduSense.Shared;
using EduSense.UI.Components;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class AnswerDistributionDonutTests : TestContext
    {
        [Fact]
        public void Shows_placeholder_when_no_data()
        {
            var cut = RenderComponent<AnswerDistributionDonut>(parameters => parameters
                .Add(p => p.Data, new List<AnswerDistributionDto>()));

            Assert.Contains("Ingen svarsdata", cut.Markup);
        }

        [Fact]
        public void Renders_one_legend_item_per_answer_and_total_count()
        {
            var data = new List<AnswerDistributionDto>
            {
                new() { AnswerDescription = "Mycket nöjd", Count = 6, Percentage = 60 },
                new() { AnswerDescription = "Missnöjd", Count = 4, Percentage = 40 }
            };

            var cut = RenderComponent<AnswerDistributionDonut>(parameters => parameters
                .Add(p => p.Data, data));

            Assert.Equal(2, cut.FindAll("li").Count);
            Assert.Contains("Mycket nöjd", cut.Markup);
            Assert.Contains("10", cut.Markup);
        }
    }
}
