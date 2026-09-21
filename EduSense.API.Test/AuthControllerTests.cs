using EduSense.API.Controllers;
using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EduSense.API.Test;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IPasswordResetService> _passwordResetServiceMock = new();
    private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock = new();
    private readonly AuthController _sut;
   
    public AuthControllerTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new AuthController(
            _userManagerMock.Object, 
            _refreshTokenRepoMock.Object, 
            _configurationMock.Object, 
            _passwordResetServiceMock.Object,
            _webHostEnvironmentMock.Object);
    }

    [Fact]
    public async Task SetInitialPassword_WhenUserDoesNotExist_ReturnsBadRequest()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("saknas"))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = new SetInitialPasswordRequestDto { UserId = "saknas", Token = "token", NewPassword = "Nytt123!" };
        var result = await _sut.SetInitialPassword(dto);

        Assert.IsType<BadRequestObjectResult>(result);
        _userManagerMock.Verify(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetInitialPassword_WhenTokenInvalid_ReturnsBadRequestWithErrors()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "ny@edusense.se" };
        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "ogiltig-token", "Nytt123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Ogiltig token" }));

        var dto = new SetInitialPasswordRequestDto { UserId = "user-1", Token = "ogiltig-token", NewPassword = "Nytt123!" };
        var result = await _sut.SetInitialPassword(dto);

        Assert.IsType<BadRequestObjectResult>(result);
        _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task SetInitialPassword_WhenSuccessful_ActivatesUserAndReturnsOk()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "ny@edusense.se", EmailConfirmed = false, IsActive = false };
        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByNameAsync("nyanvandare")).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "giltig-token", "Nytt123!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.SetUserNameAsync(user, "nyanvandare"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new SetInitialPasswordRequestDto { UserId = "user-1", Token = "giltig-token", NewPassword = "Nytt123!", UserName = "nyanvandare" };
        var result = await _sut.SetInitialPassword(dto);

        Assert.IsType<OkObjectResult>(result);
        Assert.True(user.EmailConfirmed);
        Assert.True(user.IsActive);
        _userManagerMock.Verify(m => m.SetUserNameAsync(user, "nyanvandare"), Times.Once);
        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    // Testar att ett redan upptaget användarnamn avvisas innan lösenordet ens byts
    [Fact]
    public async Task SetInitialPassword_WhenUserNameTaken_ReturnsBadRequest()
    {
        var user = new ApplicationUser { Id = "user-1", Email = "ny@edusense.se" };
        var otherUser = new ApplicationUser { Id = "user-2", Email = "annan@edusense.se", UserName = "upptaget" };
        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.FindByNameAsync("upptaget")).ReturnsAsync(otherUser);

        var dto = new SetInitialPasswordRequestDto { UserId = "user-1", Token = "giltig-token", NewPassword = "Nytt123!", UserName = "upptaget" };
        var result = await _sut.SetInitialPassword(dto);

        Assert.IsType<BadRequestObjectResult>(result);
        _userManagerMock.Verify(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}