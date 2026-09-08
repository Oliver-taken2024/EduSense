using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class OrganisationServiceTests
{
    private readonly Mock<IOrganisationRepository> _repoMock = new();
    private readonly OrganisationService _organisationService;

    public OrganisationServiceTests()
    {
        _organisationService = new OrganisationService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_MapsRepositoryModelsToDtos()
    {
        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<OrganisationModel>
            {
                new() { Id = 1, Name = "EduSense AB" }
            });

        var result = await _organisationService.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("EduSense AB", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganisationExists_ReturnsDto()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new OrganisationModel { Id = 1, Name = "EduSense AB" });

        var result = await _organisationService.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("EduSense AB", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrganisationMissing_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((OrganisationModel?)null);

        var result = await _organisationService.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryAndReturnsDto()
    {
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<OrganisationModel>()))
            .ReturnsAsync((OrganisationModel o) => { o.Id = 42; return o; });

        var result = await _organisationService.CreateAsync(new OrganisationDto { Name = "Ny Organisation" });

        Assert.Equal(42, result.Id);
        Assert.Equal("Ny Organisation", result.Name);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<OrganisationModel>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsValidationException()
    {
        var dto = new OrganisationDto { Name = "  " };

        await Assert.ThrowsAsync<ValidationException>(() => _organisationService.CreateAsync(dto));

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<OrganisationModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepositoryAndReturnsUpdatedDto()
    {
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<OrganisationModel>()))
            .ReturnsAsync((OrganisationModel o) => o);

        var result = await _organisationService.UpdateAsync(new OrganisationDto { Id = 1, Name = "Uppdaterat Namn" });

        Assert.Equal("Uppdaterat Namn", result.Name);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<OrganisationModel>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyName_ThrowsValidationException()
    {
        var dto = new OrganisationDto { Id = 1, Name = "  " };

        await Assert.ThrowsAsync<ValidationException>(() => _organisationService.UpdateAsync(dto));

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<OrganisationModel>()), Times.Never);
    }


}