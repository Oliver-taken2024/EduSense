using Bunit;
using EduSense.Shared;
using EduSense.UI.Components;
using TestContext = Bunit.TestContext;

namespace EduSense.UI.Test
{
    public class GeoDistributionTests : TestContext
    {
        [Fact]
        public void Shows_placeholder_when_no_locations_have_coordinates()
        {
            var locations = new List<OrganisationLocationDto>
            {
                new() { OrganisationName = "Skola utan koordinater" }
            };

            var cut = RenderComponent<GeoDistribution>(parameters => parameters
                .Add(p => p.Locations, locations));

            Assert.Contains("Ingen geografisk data", cut.Markup);
        }

        [Fact]
        public void Renders_one_circle_per_location_with_coordinates()
        {
            var locations = new List<OrganisationLocationDto>
            {
                new() { OrganisationName = "Skola A", Latitude = 55.6, Longitude = 13.0, AverageScore = 4.2 },
                new() { OrganisationName = "Skola B", Latitude = 55.7, Longitude = 13.2, AverageScore = 3.1 }
            };

            var cut = RenderComponent<GeoDistribution>(parameters => parameters
                .Add(p => p.Locations, locations));

            Assert.Equal(2, cut.FindAll("circle").Count);
            Assert.Contains("Skola A", cut.Markup);
            Assert.Contains("Skola B", cut.Markup);
        }
    }
}
