using System.Threading.Tasks;
using Xunit;
using EduSense.UI.Test.Helpers;
using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.Shared;
using Moq;
using Microsoft.AspNetCore.Identity;
using EduSense.API.Controllers;
using EduSense.DAL.Repositories;
using AngleSharp;

using Microsoft.AspNetCore.Mvc;

namespace EduSense.UI.Test.Helpers
{
    public class ForgotPasswordPageTest
    {
        [Fact]
        public async Task ForgotPasswordAsync_Returns_NotFound_When_Token_Is_Null()
        {
            var passwordResetServiceMock = new Mock<IPasswordResetService>();

            passwordResetServiceMock.Setup(x=>x.ForgotPasswordAsync("TestEmail")).ReturnsAsync((string)null);

            var store = new Mock<IUserStore<ApplicationUser>>();

            var userManager= new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

            var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

            var configuration = new Mock<IConfiguration>();

            var controller = new AuthController(userManager.Object, (IRefreshTokenRepository)passwordResetServiceMock.Object, (Microsoft.Extensions.Configuration.IConfiguration)refreshTokenRepository.Object, (IPasswordResetService)configuration.Object);

            var dto = new ForgotPasswordDto { Email = "TestEmail" };

            var result = await controller.ForgetPassword(dto);

            Assert.IsType<NotFoundResult>(result);





        }
    }
}
    

