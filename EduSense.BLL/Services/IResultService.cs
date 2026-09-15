using EduSense.BLL.Results;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface IResultService
    {
        Task<ResultSaveStatus> SaveAnswerAsync(SaveResultDto dto);

        Task<ResultSaveStatus> CompleteAsync(string token);

        Task<SurveyResultDto> GetResultForSurveyAsync(int surveyId);
        Task<SurveyResultDto> GetResultForDispatchAsync(int dispatchId);
        Task<IReadOnlyList<OrganisationLocationDto>> GetOrganisationOverviewAsync();
    }
}
