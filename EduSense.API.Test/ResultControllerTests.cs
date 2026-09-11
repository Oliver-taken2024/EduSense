using EduSense.API.Controllers;
using EduSense.BLL.Results;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduSense.API.Test
{
    public class ResultControllerTests
    {
        private readonly Mock<IResultService> _serviceMock = new();
        private readonly ResultController _resultController;

        public ResultControllerTests()
        {
            _resultController = new ResultController(_serviceMock.Object);
        }

        [Fact]
        public async Task SaveAnswer_Success_ReturnsOk()
        {
            var dto = new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 };
            _serviceMock.Setup(s => s.SaveAnswerAsync(dto)).ReturnsAsync(ResultSaveStatus.Success);

            var result = await _resultController.SaveAnswer(dto);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveAnswer_TokenNotFound_ReturnsNotFound()
        {
            var dto = new SaveResultDto { Token = "bad-token", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 };
            _serviceMock.Setup(s => s.SaveAnswerAsync(dto)).ReturnsAsync(ResultSaveStatus.TokenNotFound);

            var result = await _resultController.SaveAnswer(dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SaveAnswer_SurveyExpired_ReturnsBadRequest()
        {
            var dto = new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 };
            _serviceMock.Setup(s => s.SaveAnswerAsync(dto)).ReturnsAsync(ResultSaveStatus.SurveyExpired);

            var result = await _resultController.SaveAnswer(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SaveAnswer_AlreadyCompleted_ReturnsConflict()
        {
            var dto = new SaveResultDto { Token = "token-1", SurveyQuestionId = 30, QuestionAnswerOptionId = 20 };
            _serviceMock.Setup(s => s.SaveAnswerAsync(dto)).ReturnsAsync(ResultSaveStatus.AlreadyCompleted);

            var result = await _resultController.SaveAnswer(dto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task SaveAnswer_InvalidQuestionOrAnswer_ReturnsBadRequest()
        {
            var dto = new SaveResultDto { Token = "token-1", SurveyQuestionId = 999, QuestionAnswerOptionId = 999 };
            _serviceMock.Setup(s => s.SaveAnswerAsync(dto)).ReturnsAsync(ResultSaveStatus.InvalidQuestionOrAnswer);

            var result = await _resultController.SaveAnswer(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Complete_Success_ReturnsOk()
        {
            _serviceMock.Setup(s => s.CompleteAsync("token-1")).ReturnsAsync(ResultSaveStatus.Success);

            var result = await _resultController.Complete("token-1");

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Complete_TokenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.CompleteAsync("bad-token")).ReturnsAsync(ResultSaveStatus.TokenNotFound);

            var result = await _resultController.Complete("bad-token");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
