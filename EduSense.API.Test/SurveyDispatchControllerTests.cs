using EduSense.API.Controllers;
using EduSense.BLL.Exceptions;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace EduSense.API.Test
{
    public class SurveyDispatchControllerTests
    {
        // Mockar ISurveyDispatchService för att testa SurveyDispatchController
        // utan att behöva en riktig implementation.
        private readonly Mock<ISurveyDispatchService> _serviceMock = new();

        // Skapar en instans av SurveyDispatchController med den mockade servicen.
        private readonly SurveyDispatchController _surveyDispatchController;

        public SurveyDispatchControllerTests()
        {
            _surveyDispatchController = new SurveyDispatchController(_serviceMock.Object);
            SetUser("authenticated-user-id");
        }

        // Simulerar en inloggad användare, ungefär som riktiga requests har efter inloggning.
        private void SetUser(string? userId)
        {
            var claims = new List<Claim>();
            if (userId is not null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            var identity = new ClaimsIdentity(claims, userId is not null ? "TestAuth" : null);
            var principal = new ClaimsPrincipal(identity);

            _surveyDispatchController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]

        //Kolla att GetAll returnerar alla dispatches när surveyId är null
        public async Task GetAll_WithoutSurveyId_ReturnsAllDispatches()
        {
            var dispatches = new List<SurveyDispatchDto>
            {
                new() { Id = 1, SurveyId = 1, SurveyTitle = "Enkät" }
            };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(dispatches);

            var result = await _surveyDispatchController.GetAll(null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dispatches, okResult.Value);
            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
            _serviceMock.Verify(s => s.GetAllForSurveyAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]

        //Kolla att GetAll returnerar dispatches för en specifik survey när surveyId är satt
        public async Task GetAll_WithSurveyId_ReturnsDispatchesForSurvey()
        {
            var dispatches = new List<SurveyDispatchDto>
            {
                new() { Id = 2, SurveyId = 5, SurveyTitle = "Enkät" }
            };
            _serviceMock.Setup(s => s.GetAllForSurveyAsync(5)).ReturnsAsync(dispatches);

            var result = await _surveyDispatchController.GetAll(5);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dispatches, okResult.Value);
            _serviceMock.Verify(s => s.GetAllForSurveyAsync(5), Times.Once);
            _serviceMock.Verify(s => s.GetAllAsync(), Times.Never);
        }

        [Fact]

        //Kolla att GetById returnerar dispatch när den finns
        public async Task GetById_WhenFound_ReturnsOk()
        {
            var dto = new SurveyDispatchDto { Id = 1, SurveyId = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await _surveyDispatchController.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dto, okResult.Value);
        }

        [Fact]

        //Kolla att GetById returnerar NotFound när dispatch inte finns
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((SurveyDispatchDto?)null);

            var result = await _surveyDispatchController.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]

        //Kolla att Create returnerar CreatedAtAction när dto är giltig
        public async Task Create_WithValidDto_ReturnsCreatedAtAction()
        {
            var dto = new SurveyDispatchSaveDto
            {
                SurveyId = 1,
                ResponseDeadline = DateTime.UtcNow.AddDays(30),
                SentByUserId = "user-1",
                Respondents = [new RespondentInviteDto { Email = "test@test.se", Segment = RespondentSegmentDto.Grade7To9 }]
            };
            var created = new SurveyDispatchDto { Id = 7, SurveyId = 1 };
            _serviceMock.Setup(s => s.CreateAndSendAsync(dto)).ReturnsAsync(created);

            var result = await _surveyDispatchController.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(SurveyDispatchController.GetById), createdResult.ActionName);
            Assert.Equal(7, createdResult.RouteValues!["id"]);
            Assert.Equal(created, createdResult.Value);
        }

        [Fact]

        //Kolla att Create returnerar BadRequest när validering misslyckas
        public async Task Create_WhenValidationFails_ReturnsBadRequest()
        {
            var dto = new SurveyDispatchSaveDto
            {
                SurveyId = 1,
                Respondents = []
            };
            var errors = new List<string> { "Minst en respondent krävs." };
            _serviceMock.Setup(s => s.CreateAndSendAsync(dto))
                .ThrowsAsync(new SurveyValidationException(errors));

            var result = await _surveyDispatchController.Create(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(errors, badRequest.Value);
        }

        [Fact]

        //Kolla att Delete returnerar NoContent när dispatch finns och tas bort
        public async Task Delete_WhenFound_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _surveyDispatchController.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]

        //Kolla att Delete returnerar NotFound när dispatch inte finns
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

            var result = await _surveyDispatchController.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}