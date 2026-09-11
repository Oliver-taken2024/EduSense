using EduSense.BLL.Results;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface IResultService
    {
        Task<ResultSaveStatus> SaveAnswerAsync(SaveResultDto dto);

        Task<ResultSaveStatus> CompleteAsync(string token);
    }
}
