using EduSense.BLL.Exceptions;
using EduSense.DAL.Models;
using EduSense.DAL.Repositories;
using EduSense.Shared;
using Microsoft.Extensions.Configuration;

namespace EduSense.BLL.Services
{
    public class SurveyDispatchService : ISurveyDispatchService
    {
        private readonly ISurveyDispatchRepository _dispatchRepository;

        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public SurveyDispatchService(ISurveyDispatchRepository dispatchRepository, IEmailSender emailSender, IConfiguration configuration)
        {
            _dispatchRepository = dispatchRepository;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<SurveyDispatchDto>> GetAllAsync()
        {
            var dispatches = await _dispatchRepository.GetAllAsync();
            return dispatches.Select(ToDto).ToList();
        }

        public async Task<IReadOnlyList<SurveyDispatchDto>> GetAllForSurveyAsync(int surveyId)
        {
            var dispatches = await _dispatchRepository.GetAllForSurveyAsync(surveyId);
            return dispatches.Select(ToDto).ToList();
        }

        public async Task<SurveyDispatchDto?> GetByIdAsync(int id)
        {
            var dispatch = await _dispatchRepository.GetByIdAsync(id);
            return dispatch is null ? null : ToDto(dispatch);
        }

        public async Task<SurveyDispatchDto> CreateAndSendAsync(SurveyDispatchSaveDto dto)
        {
            await ValidateAsync(dto);

            var dispatch = new SurveyDispatchModel
            {
                SurveyId = dto.SurveyId,
                ResponseDeadline = DateTime.SpecifyKind(dto.ResponseDeadline, DateTimeKind.Utc),
                SentByUserId = dto.SentByUserId,
                SentAt = DateTime.UtcNow,
                Respondents = dto.Respondents
                    .DistinctBy(r => r.Email, StringComparer.OrdinalIgnoreCase)
                    .Select(r => new RespondentModel
                    {
                        Email = r.Email,
                        Segment = MapSegment(r.Segment),
                        Token = GenerateToken()
                    })
                    .ToList()
            };

            await _dispatchRepository.AddAsync(dispatch);
            
            var created = await _dispatchRepository.GetByIdAsync(dispatch.Id);
            await SendInvitationsAsync(created!);
            return ToDto(created!);
        }

        private async Task SendInvitationsAsync(SurveyDispatchModel dispatch)
        {
            var surveyTitle = dispatch.Survey?.Title ?? "Enkät";
            var clientBaseUrl = _configuration["ClientApp:BaseUrl"];

            foreach (var respondent in dispatch.Respondents)
            {
                var link = $"{clientBaseUrl}/enkat/{Uri.EscapeDataString(respondent.Token)}";

                var subject = $"Inbjudan att svara på: {surveyTitle}";
                var htmlBody =
                    $"<p>Du har blivit inbjuden att svara på enkäten <strong>{surveyTitle}</strong>.</p>" +
                    $"<p><a href=\"{link}\">Klicka här för att svara</a></p>" +
                    $"<p>Sista svarsdatum: {dispatch.ResponseDeadline:yyyy-MM-dd}.</p>";

                await _emailSender.SendAsync(respondent.Email, subject, htmlBody);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _dispatchRepository.DeleteAsync(id);
        }

        private static SurveyDispatchDto ToDto(SurveyDispatchModel dispatch)
        {
            return new SurveyDispatchDto
            {
                Id = dispatch.Id,
                SurveyId = dispatch.SurveyId,
                SurveyTitle = dispatch.Survey?.Title,
                ResponseDeadline = dispatch.ResponseDeadline,
                SentAt = dispatch.SentAt,
                SentByUserId = dispatch.SentByUserId,
                RespondentCount = dispatch.Respondents.Count,
                ResponseCount = dispatch.Respondents.Count(r => r.TokenIsUsed)
            };
        }

        private async Task ValidateAsync(SurveyDispatchSaveDto dto)
        {
            var errors = new List<string>();

            var surveyExists = await _dispatchRepository.SurveyExistsAsync(dto.SurveyId);
            if (!surveyExists)
            {
                errors.Add("Enkätmallen som utskicket avser finns inte.");
            }

            if (dto.ResponseDeadline <= DateTime.UtcNow)
            {
                errors.Add("Svarsdeadline måste vara ett framtida datum.");
            }

            var distinctEmails = dto.Respondents
                           .Select(r => r.Email)
                           .Where(e => !string.IsNullOrWhiteSpace(e))
                           .Distinct(StringComparer.OrdinalIgnoreCase)
                           .ToList();

            if (distinctEmails.Count == 0)
            {
                errors.Add("Minst en respondent måste anges.");
            }

            if (errors.Count > 0)
            {
                throw new SurveyValidationException(errors);
            }
        }

        private static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static RespondentSegment MapSegment(RespondentSegmentDto segment) => segment switch
        {
            RespondentSegmentDto.GradeFTo6 => RespondentSegment.GradeFTo6,
            RespondentSegmentDto.Grade7To9 => RespondentSegment.Grade7To9,
            RespondentSegmentDto.Gymnasiet => RespondentSegment.Gymnasiet,
            RespondentSegmentDto.Vuxenutbildning => RespondentSegment.Vuxenutbildning,
            RespondentSegmentDto.Personal => RespondentSegment.Personal,
            _ => throw new ArgumentOutOfRangeException(nameof(segment))
        };
    }
}