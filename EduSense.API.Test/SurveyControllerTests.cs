using EduSense.API.Controllers;
using EduSense.BLL.Exceptions;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduSense.API.Test
{
    public class SurveyControllerTests
    {
        private readonly Mock<ISurveyService> _serviceMock = new();
        private readonly SurveyController _surveyController;

        public SurveyControllerTests()
        {
            _surveyController = new SurveyController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithSurveys()
        {
            var surveys = new List<SurveyDto> { new() { Id = 1, Title = "Enkät 1" } };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(surveys);

            var result = await _surveyController.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(surveys, okResult.Value);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((SurveyDto?)null);

            var result = await _surveyController.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsOkWithSurvey()
        {
            var survey = new SurveyDto { Id = 1, Title = "Enkät 1" };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(survey);

            var result = await _surveyController.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(survey, okResult.Value);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsValidationException_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<SurveySaveDto>()))
                .ThrowsAsync(new SurveyValidationException(["Titeln är upptagen."]));

            var result = await _surveyController.Create(new SurveySaveDto { Title = "X" });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_WhenSuccessful_ReturnsCreatedAtAction()
        {
            var created = new SurveyDto { Id = 1, Title = "Ny enkät" };
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<SurveySaveDto>())).ReturnsAsync(created);

            var result = await _surveyController.Create(new SurveySaveDto { Title = "Ny enkät" });

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Same(created, createdResult.Value);
        }

        [Fact]
        public async Task Update_WhenServiceThrowsValidationException_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<SurveySaveDto>()))
                .ThrowsAsync(new SurveyValidationException(["Titeln är upptagen."]));

            var result = await _surveyController.Update(1, new SurveySaveDto { Title = "X" });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Update_WhenSurveyMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.UpdateAsync(99, It.IsAny<SurveySaveDto>())).ReturnsAsync((SurveyDto?)null);

            var result = await _surveyController.Update(99, new SurveySaveDto { Title = "X" });

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_WhenSuccessful_ReturnsOkWithUpdatedSurvey()
        {
            var updated = new SurveyDto { Id = 1, Title = "Uppdaterad titel" };
            _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<SurveySaveDto>())).ReturnsAsync(updated);

            var result = await _surveyController.Update(1, new SurveySaveDto { Title = "Uppdaterad titel" });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(updated, okResult.Value);
        }

        [Fact]
        public async Task Delete_WhenSuccessful_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _surveyController.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_WhenSurveyMissing_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

            var result = await _surveyController.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
