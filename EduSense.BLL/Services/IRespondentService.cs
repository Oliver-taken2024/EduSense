using System;
using System.Collections.Generic;
using System.Text;
using EduSense.BLL.Results;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface IRespondentService
    {
        Task<RespondentResult<RespondentSurveyDto>> GetSurveyByTokenAsync(string token);
    }
}
