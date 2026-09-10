using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduSense.BLL.Services;
using EduSense.BLL.Results;
using EduSense.Shared;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly IResultService _resultService;

        public ResultController(IResultService resultService)
        {
            _resultService = resultService;
        }

        // POST /api/result - sparar/uppdaterar ett svar för en fråga. Inget inloggningskrav, token i body är auktoriseringen.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SaveAnswer(SaveResultDto dto)
        {
            var status = await _resultService.SaveAnswerAsync(dto);
            return MapStatus(status);
        }

        // POST /api/result/{token}/complete - markerar enkäten som färdigbesvarad.
        [HttpPost("{token}/complete")]
        [AllowAnonymous]
        public async Task<IActionResult> Complete(string token)
        {
            var status = await _resultService.CompleteAsync(token);
            return MapStatus(status);
        }

        private IActionResult MapStatus(ResultSaveStatus status) => status switch
        {
            ResultSaveStatus.Success => Ok(),
            ResultSaveStatus.TokenNotFound => NotFound(),
            ResultSaveStatus.SurveyExpired => BadRequest("Enkäten har gått ut."),
            ResultSaveStatus.AlreadyCompleted => Conflict("Enkäten är redan besvarad."),
            ResultSaveStatus.InvalidQuestionOrAnswer => BadRequest("Ogiltig fråga eller svarsalternativ."),
            _ => throw new InvalidOperationException($"Okänd status: {status}")
        };
    }
}
