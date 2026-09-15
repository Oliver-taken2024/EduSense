using Bunit;
using EduSense.Shared;
using EduSense.UI.Components;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class TrendChartTests : TestContext
    {
        [Fact]
        public void Shows_placeholder_when_no_points()
        {
            var cut = RenderComponent<TrendChart>(parameters => parameters
                .Add(p => p.Points, new List<TrendPointDto>()));

            Assert.Contains("Ingen trenddata", cut.Markup);
        }

        [Fact]
        public void Renders_one_polyline_per_category()
        {
            var points = new List<TrendPointDto>
            {
                new() { PeriodStart = new DateTime(2026, 1, 1), CategoryName = "Trivsel", AverageScore = 4 },
                new() { PeriodStart = new DateTime(2026, 2, 1), CategoryName = "Trivsel", AverageScore = 3.5 },
                new() { PeriodStart = new DateTime(2026, 1, 1), CategoryName = "Lärande", AverageScore = 4.5 },
                new() { PeriodStart = new DateTime(2026, 2, 1), CategoryName = "Lärande", AverageScore = 4.2 }
            };

            var cut = RenderComponent<TrendChart>(parameters => parameters
                .Add(p => p.Points, points));

            Assert.Equal(2, cut.FindAll("polyline").Count);
            Assert.Contains("Trivsel", cut.Markup);
            Assert.Contains("Lärande", cut.Markup);
        }
    }
}
