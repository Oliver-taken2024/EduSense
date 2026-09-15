using EduSense.BLL.Services;
using EduSense.Shared;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.BLL.Exceptions;
using Moq;
using Xunit;

namespace EduSense.BLL.Test
{
    public class SurveyServiceTests
    {
        private readonly Mock<ISurveyRepository> _surveyrepoMock = new();
        private readonly SurveyService _surveyService;

        public SurveyServiceTests()
        {
            _surveyService = new SurveyService(_surveyrepoMock.Object);
        }

        private static SurveyModel MakeSurvey(int id = 1, string title = "Enkät", int orgId = 1) => new()
        {
            Id = id,
            Title = title,
            OrganisationId = orgId,
            CreatedByUserId = "user-1",
        };

        [Fact]
        public async Task CreateAsync_WithMissingQuestionIds_ThrowsSurveyValidationException()
        {
            var dto = new SurveySaveDto
            {
                Title = "Ny Enkät",
                OrganisationId = 1,
                CreatedByUserId = "user-1",
                QuestionIds = [1, 2]
            };

            _surveyrepoMock.Setup(r => r.GetExistingQuestionIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<int> { 1 });
            _surveyrepoMock.Setup(r => r.TitleExistsAsync(dto.Title, dto.OrganisationId, null))
                .ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<SurveyValidationException>(() => _surveyService.CreateAsync(dto));

            Assert.Contains(ex.Errors, e => e.Contains('2'));
            _surveyrepoMock.Verify(r => r.AddAsync(It.IsAny<SurveyModel>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicate_ThrowsSurveyValidationException()
        {
            var dto = new SurveySaveDto
            {
                Title = "Dubblett",
                OrganisationId = 1,
                CreatedByUserId = "user-1",
                QuestionIds = []
            };

            _surveyrepoMock.Setup(r => r.TitleExistsAsync(dto.Title, dto.OrganisationId, null))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<SurveyValidationException>(() => _surveyService.CreateAsync(dto));

            _surveyrepoMock.Verify(r => r.AddAsync(It.IsAny<SurveyModel>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithValidDto_AddsSurveyAndReturnsDto()
        {
            var dto = new SurveySaveDto
            {
                Title = "Ny enkät",
                OrganisationId = 1,
                CreatedByUserId = "user-1",
                QuestionIds = [1, 1, 2]
            };

            _surveyrepoMock.Setup(r => r.GetExistingQuestionIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<int> { 1, 2 });
            _surveyrepoMock.Setup(r => r.TitleExistsAsync(dto.Title, dto.OrganisationId, null))
                .ReturnsAsync(false);

            SurveyModel? captured = null;
            _surveyrepoMock.Setup(r => r.AddAsync(It.IsAny<SurveyModel>()))
                .Callback<SurveyModel>(s => { s.Id = 7; captured = s; })
                .Returns(Task.CompletedTask);
            _surveyrepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(() => captured);

            var result = await _surveyService.CreateAsync(dto);

            Assert.Equal(7, result.Id);
            Assert.Equal("Ny enkät", result.Title);
            Assert.Equal(2, captured!.SurveyQuestions.Count);
        }

        [Fact]
        public async Task UpdateAsync_WhenSurveyNotFound_ReturnsNull()
        {
            _surveyrepoMock.Setup(r => r.GetTrackedByIdAsync(99)).ReturnsAsync((SurveyModel?)null);

            var result = await _surveyService.UpdateAsync(99, new SurveySaveDto { Title = "X", QuestionIds = [] });

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ValidatesAgainstOwnIdExcluded()
        {
            var survey = MakeSurvey(id: 5);
            _surveyrepoMock.Setup(r => r.GetTrackedByIdAsync(5)).ReturnsAsync(survey);
            _surveyrepoMock.Setup(r => r.TitleExistsAsync(It.IsAny<string>(), It.IsAny<int>(), 5))
                .ReturnsAsync(false);
            _surveyrepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(survey);

            var dto = new SurveySaveDto { Title = "Uppdaterad", OrganisationId = 1, QuestionIds = [] };

            await _surveyService.UpdateAsync(5, dto);

            _surveyrepoMock.Verify(r => r.TitleExistsAsync(dto.Title, dto.OrganisationId, 5), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesSurveyQuestions()
        {
            var survey = MakeSurvey(id: 5);
            survey.SurveyQuestions.Add(new SurveyQuestionModel { Id = 1, QuestionId = 1, SurveyId = 5 });

            _surveyrepoMock.Setup(r => r.GetTrackedByIdAsync(5)).ReturnsAsync(survey);
            _surveyrepoMock.Setup(r => r.GetExistingQuestionIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<int> { 2, 3 });
            _surveyrepoMock.Setup(r => r.TitleExistsAsync(It.IsAny<string>(), It.IsAny<int>(), 5))
                .ReturnsAsync(false);
            _surveyrepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(survey);

            var dto = new SurveySaveDto { Title = "T", OrganisationId = 1, QuestionIds = [2, 3] };

            await _surveyService.UpdateAsync(5, dto);

            Assert.Equal(2, survey.SurveyQuestions.Count);
            Assert.DoesNotContain(survey.SurveyQuestions, q => q.QuestionId == 1);
            _surveyrepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            _surveyrepoMock.Setup(r => r.DeleteAsync(3)).ReturnsAsync(true);

            var result = await _surveyService.DeleteAsync(3);

            Assert.True(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            _surveyrepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SurveyModel?)null);

            var result = await _surveyService.GetByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_MapsQuestionIdAndOrganisationCorrectly()
        {
            var survey = MakeSurvey(id: 1);
            survey.Organisation = new OrganisationModel { Id = 9, Name = "Acme" };
            survey.SurveyQuestions.Add(new SurveyQuestionModel
            {
                Id = 11,
                QuestionId = 4,
                Question = new QuestionModel { Id = 4, Text = "Fråga?", CreatedByUserId = "u" }
            });

            _surveyrepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SurveyModel> { survey });

            var result = await _surveyService.GetAllAsync();

            var resultDto = result.Single();
            Assert.Equal("Acme", resultDto.Organisation?.Name);
            Assert.Equal(4, resultDto.SurveyQuestions.Single().QuestionId);
            Assert.Equal("Fråga?", resultDto.SurveyQuestions.Single().QuestionText);
        }
    }
}