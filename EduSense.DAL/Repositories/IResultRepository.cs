using EduSense.DAL.Models;

namespace EduSense.DAL.Repositories
{
    public interface IResultRepository
    {
        Task<ResponseModel?> GetTrackedByRespondentAndQuestionAsync(int respondentId, int surveyQuestionId);

        Task AddAsync(ResponseModel response);

        Task SaveChangesAsync();

        Task<IReadOnlyList<RespondentModel>> GetRespondentsForSurveyAsync(int surveyId);
        Task<IReadOnlyList<RespondentModel>> GetRespondentsForDispatchAsync(int dispatchId);
        Task<IReadOnlyList<OrganisationModel>> GetOrganisationsWithResponsesAsync();
        Task<int?> GetPreviousDispatchIdAsync(int dispatchId);
    }
}
