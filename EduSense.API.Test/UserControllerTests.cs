using EduSense.API.Controllers;
using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class UsersControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _configurationMock.Setup(c => c["ClientApp:BaseUrl"]).Returns("https://localhost:7289");

        _sut = new UsersController(_userManagerMock.Object, _emailSenderMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Invite_WithInvalidRole_ReturnsBadRequest()
    {
        var dto = new InviteUserRequestDto { Email = "ny@edusense.se", Role = "Respondent" };

        var result = await _sut.Invite(dto);

        var badRequest = Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        _userManagerMock.Verify(m => m.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Invite_WhenUserDoesNotExist_CreatesUserAssignsRoleAndSendsEmail()
    {
        var dto = new InviteUserRequestDto { Email = "ny@edusense.se", Role = "Admin" };

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), dto.Role))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("dummy-token");

        var result = await _sut.Invite(dto);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        _userManagerMock.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u => u.Email == dto.Email)), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"), Times.Once);
        _emailSenderMock.Verify(e => e.SendAsync(dto.Email, It.IsAny<string>(), It.Is<string>(body => body.Contains("dummy-token") || body.Contains("skapa-losenord"))), Times.Once);
    }

    [Fact]
    public async Task Invite_WhenUserCreationFails_ReturnsBadRequest()
    {
        var dto = new InviteUserRequestDto { Email = "ny@edusense.se", Role = "Admin" };

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Ogiltig e-post" }));

        var result = await _sut.Invite(dto);

        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);
        _emailSenderMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Invite_WhenUserExistsWithDifferentRole_ReplacesRoleAndResendsInvite()
    {
        var dto = new InviteUserRequestDto { Email = "befintlig@edusense.se", Role = "Admin" };
        var existingUser = new ApplicationUser { Id = "user-1", Email = dto.Email };

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync(new List<string> { "Analyst" });
        _userManagerMock.Setup(m => m.RemoveFromRolesAsync(existingUser, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(existingUser, "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(existingUser))
            .ReturnsAsync("new-token");

        var result = await _sut.Invite(dto);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _userManagerMock.Verify(m => m.RemoveFromRolesAsync(existingUser, It.Is<IEnumerable<string>>(r => r.Contains("Analyst"))), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(existingUser, "Admin"), Times.Once);
    }

    [Fact]
    public async Task Invite_WhenUserAlreadyHasRequestedRole_DoesNotReassignRole()
    {
        var dto = new InviteUserRequestDto { Email = "befintlig@edusense.se", Role = "Admin" };
        var existingUser = new ApplicationUser { Id = "user-1", Email = dto.Email };

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(existingUser);
        _userManagerMock.Setup(m => m.GetRolesAsync(existingUser))
            .ReturnsAsync(new List<string> { "Admin" });
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(existingUser))
            .ReturnsAsync("new-token");

        var result = await _sut.Invite(dto);

        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
        _userManagerMock.Verify(m => m.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }
}