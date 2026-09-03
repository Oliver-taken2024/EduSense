using System;
using System.Collections.Generic;
using System.Text;

using EduSense.DAL.Models;
using EduSense.Shared;

namespace EduSense.BLL.Services
{
    public interface ISurveyService
    {
        Task<IReadOnlyList<SurveyDto>> GetAllAsync();

        Task<SurveyDto?> GetByIdAsync(int id);

        Task<SurveyDto> CreateAsync(SurveySaveDto dto);

        Task<SurveyDto?> UpdateAsync(int id, SurveySaveDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
