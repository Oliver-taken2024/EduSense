using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _repoMock = new();
    private readonly QuestionService _sut;

    public QuestionServiceTests()
    {
        _sut = new QuestionService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyText_ThrowsValidationException()
    {
        var dto = new QuestionDto { Text = "  " };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(dto));

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<QuestionModel>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidText_CallsRepositoryAndReturnsDto()
    {
        var dto = new QuestionDto { Text = "Hur trivs du?", CreatedByUserId = "user-1" };
        _repoMock
            .Setup(r => r.CreateAsync(It.IsAny<QuestionModel>()))
            .ReturnsAsync((QuestionModel q) => { q.Id = 42; return q; });

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(42, result.Id);
        Assert.Equal("Hur trivs du?", result.Text);
    }

    [Fact]
    public async Task UpdateAsync_WhenQuestionExists_UpdatesTextAndReturnsDto()
    {
        var question = new QuestionModel { Id = 1, Text = "Gammal text", CreatedByUserId = "user-1" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(question);
        _repoMock.Setup(r => r.UpdateAsync(question)).ReturnsAsync(question);

        var result = await _sut.UpdateAsync(1, new QuestionDto { Text = "Ny text" });

        Assert.Equal("Ny text", result?.Text);
        _repoMock.Verify(r => r.UpdateAsync(question), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenQuestionNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((QuestionModel?)null);

        var result = await _sut.UpdateAsync(99, new QuestionDto { Text = "Text" });

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyText_ThrowsValidationException()
    {
        var dto = new QuestionDto { Text = "  " };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(1, dto));

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<QuestionModel>()), Times.Never);
    }


    [Fact]
    public async Task DeleteAsync_WhenQuestionExists_ReturnsTrue()
    {
        var question = new QuestionModel { Id = 1, Text = "X", CreatedByUserId = "u" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(question);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteAsync(question), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenQuestionNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((QuestionModel?)null);

        var result = await _sut.DeleteAsync(99);

        Assert.False(result);
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<QuestionModel>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_MapsOrganisationCorrectly_WhenPresent()
    {
        _repoMock.Setup(r => r.GetAllWithOrganisationAsync()).ReturnsAsync(new List<QuestionWithOrganisationModel>
        {
            new()
            {
                Question = new QuestionModel { Id = 1, Text = "Q1", CreatedByUserId = "u1" },
                Organisation = new OrganisationModel { Id = 5, Name = "Acme AB" }
            }
        });

        var result = await _sut.GetAllAsync();

        Assert.Equal("Acme AB", result.Single().Organisation?.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenQuestionNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdWithOrganisationAsync(99)).ReturnsAsync((QuestionWithOrganisationModel?)null);

        var result = await _sut.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenQuestionFound_MapsOrganisationCorrectly()
    {
        var withOrg = new QuestionWithOrganisationModel
        {
            Question = new QuestionModel { Id = 1, Text = "Fråga 1", CreatedByUserId = "user-1" },
            Organisation = new OrganisationModel { Id = 5, Name = "Acme AB" }
        };
        _repoMock.Setup(r => r.GetByIdWithOrganisationAsync(1)).ReturnsAsync(withOrg);

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Fråga 1", result.Text);
        Assert.Equal("Acme AB", result.Organisation?.Name);
    }
}