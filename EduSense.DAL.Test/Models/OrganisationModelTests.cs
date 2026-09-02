using EduSense.DAL.Models;

namespace EduSense.DAL.Test.Models;

public class OrganisationModelTests
{
    // Test av OrganisationModel 
    [Fact]
    public void Collections_are_initialized()
    {
        var organisation = new OrganisationModel
        {
            Name = "EduSense"
        };

        //kolla att OrganisationUsers och Surveys inte är null och att de inleds som tomma.
        Assert.NotNull(organisation.OrganisationUsers);
        Assert.NotNull(organisation.Surveys);
        Assert.Empty(organisation.OrganisationUsers);
        Assert.Empty(organisation.Surveys);
    }
}
