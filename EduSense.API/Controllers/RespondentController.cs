using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduSense.BLL.Services;
using EduSense.BLL.Results;
using EduSense.Shared;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RespondentController : ControllerBase
    {
        private readonly IRespondentService _respondentService;

        public RespondentController(IRespondentService respondentService)
        {
            _respondentService = respondentService;
        }

        [HttpGet("{token}")]
        [AllowAnonymous]
        public async Task<ActionResult<RespondentSurveyDto>> GetSurveyByToken(string token)
        {
            var result = await _respondentService.GetSurveyByTokenAsync(token);

            return result.Status switch
            {
                RespondentResultStatus.Success => Ok(result.Value),
                RespondentResultStatus.TokenNotFound => NotFound(),
                RespondentResultStatus.SurveyExpired => BadRequest(new[] { "Enkäten har gått ut." }),
                _ => throw new InvalidOperationException($"Okänd status: {result.Status}")
            };
        }
    }
}
