using EduSense.DAL.Models;

namespace EduSense.DAL.Test.Models;

public class ApplicationUserTests
{
    [Fact]
    public void IsActive_defaults_to_true()
    {
        var user = new ApplicationUser();

        Assert.True(user.IsActive);
        Assert.Null(user.DisplayName);
    }
}
