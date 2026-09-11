using EduSense.API.Controllers;
using EduSense.BLL.Results;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduSense.API.Test
{
    public class RespondentControllerTests
    {
        private readonly Mock<IRespondentService> _serviceMock = new();
        private readonly RespondentController _respondentController;

        public RespondentControllerTests()
        {
            _respondentController = new RespondentController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetSurveyByToken_Success_ReturnsOkWithSurvey()
        {
            var dto = new RespondentSurveyDto { Title = "Enkät" };
            _serviceMock.Setup(s => s.GetSurveyByTokenAsync("token-1"))
                .ReturnsAsync(RespondentResult<RespondentSurveyDto>.Success(dto));

            var result = await _respondentController.GetSurveyByToken("token-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(dto, okResult.Value);
        }

        [Fact]
        public async Task GetSurveyByToken_TokenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetSurveyByTokenAsync("bad-token"))
                .ReturnsAsync(RespondentResult<RespondentSurveyDto>.Failure(RespondentResultStatus.TokenNotFound));

            var result = await _respondentController.GetSurveyByToken("bad-token");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetSurveyByToken_SurveyExpired_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.GetSurveyByTokenAsync("expired-token"))
                .ReturnsAsync(RespondentResult<RespondentSurveyDto>.Failure(RespondentResultStatus.SurveyExpired));

            var result = await _respondentController.GetSurveyByToken("expired-token");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
