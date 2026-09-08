using EduSense.API.Controllers;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class OrganisationControllerTests
{
    private readonly Mock<IOrganisationService> _serviceMock = new();
    private readonly OrganisationController _sut;

    public OrganisationControllerTests()
    {
        _sut = new OrganisationController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithOrganisations()
    {
        var organisations = new List<OrganisationDto> { new() { Id = 1, Name = "EduSense AB" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(organisations);

        var result = await _sut.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(organisations, okResult.Value);
    }

    [Fact]
    public async Task Post_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<OrganisationDto>()))
            .ThrowsAsync(new ValidationException("Namn får inte vara tomt."));

        var result = await _sut.Post(new OrganisationDto { Name = "" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Post_WhenSuccessful_ReturnsCreatedAtAction()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<OrganisationDto>()))
            .ReturnsAsync(new OrganisationDto { Id = 1, Name = "EduSense AB" });

        var result = await _sut.Post(new OrganisationDto { Name = "EduSense AB" });

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Put_WhenIdMismatch_ReturnsBadRequest()
    {
        var result = await _sut.Put(1, new OrganisationDto { Id = 2, Name = "X" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<OrganisationDto>()), Times.Never);
    }

    [Fact]
    public async Task Put_WhenServiceThrowsValidationException_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<OrganisationDto>()))
            .ThrowsAsync(new ValidationException("Namn får inte vara tomt."));

        var result = await _sut.Put(1, new OrganisationDto { Id = 1, Name = "" });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Put_WhenSuccessful_ReturnsOkWithUpdatedOrganisation()
    {
        var updatedDto = new OrganisationDto { Id = 1, Name = "Uppdaterat Namn" };
        _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<OrganisationDto>())).ReturnsAsync(updatedDto);

        var result = await _sut.Put(1, new OrganisationDto { Id = 1, Name = "Uppdaterat Namn" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(updatedDto, okResult.Value);
    }
}