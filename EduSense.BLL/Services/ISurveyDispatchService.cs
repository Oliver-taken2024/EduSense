using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface ISurveyDispatchService
    {

        // Hämtar alla utskick för en viss enkät, inklusive information om respondenter och deras svar.
        Task<IReadOnlyList<SurveyDispatchDto>> GetAllForSurveyAsync(int surveyId);

        Task<SurveyDispatchDto?> GetByIdAsync(int id);

        // Skapar ett utskick: kopplar mallen till en deadline och en lista respondenter,
        // genererar tokens och (via en mailtjänst) skickar inbjudningar.
        Task<SurveyDispatchDto> CreateAndSendAsync(SurveyDispatchSaveDto dto);

        Task<bool> DeleteAsync(int id);
    }
}