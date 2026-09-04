using EduSense.API.Controllers;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class QuestionControllerTests
{
    private readonly Mock<IQuestionService> _serviceMock = new();
    private readonly QuestionController _sut;

    public QuestionControllerTests()
    {
        _sut = new QuestionController(_serviceMock.Object);
    }

    [Fact]
    public async Task Post_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<QuestionDto>()))
            .ThrowsAsync(new ValidationException("Frågetext får inte vara tom."));

        var result = await _sut.Post(new QuestionDto { Text = "" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Post_WhenSuccessful_ReturnsCreatedAtAction()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<QuestionDto>()))
            .ReturnsAsync(new QuestionDto { Id = 1, Text = "X" });

        var result = await _sut.Post(new QuestionDto { Text = "X" });

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Put_WhenQuestionMissing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<QuestionDto>())).ReturnsAsync((QuestionDto?)null);

        var result = await _sut.Put(1, new QuestionDto { Text = "X" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Put_WhenSuccessful_ReturnsOkWithUpdatedQuestion()
    {
        var updatedDto = new QuestionDto { Id = 1, Text = "Uppdaterad text" };
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<QuestionDto>())).ReturnsAsync(updatedDto);

        var result = await _sut.Put(1, new QuestionDto { Text = "Uppdaterad text" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(updatedDto, okResult.Value);
    }

    [Fact]
    public async Task Put_WhenValid_ReturnsOK()
    {

    }

    [Fact]
    public async Task Delete_WhenSuccessful_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenQuestionMissing_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);

        var result = await _sut.Delete(1);

        Assert.IsType<NotFoundResult>(result);
    }
}