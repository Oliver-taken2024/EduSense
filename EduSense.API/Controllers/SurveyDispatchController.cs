using EduSense.BLL.Exceptions;
using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyDispatchController : ControllerBase
    {
        private readonly ISurveyDispatchService _surveyDispatchService;

        public SurveyDispatchController(ISurveyDispatchService surveyDispatchService)
        {
            _surveyDispatchService = surveyDispatchService;

        }

        [HttpGet]
        [Authorize(Policy = "AdminOrAnalyst")]
        public async Task<ActionResult<IReadOnlyList<SurveyDispatchDto>>> GetAll([FromQuery] int? surveyId)
        {
            var dispatches = surveyId.HasValue
                ? await _surveyDispatchService.GetAllForSurveyAsync(surveyId.Value)
                : await _surveyDispatchService.GetAllAsync();
            return Ok(dispatches);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrAnalyst")]
        public async Task<ActionResult<SurveyDispatchDto>> GetById(int id)
        {
            var dispatch = await _surveyDispatchService.GetByIdAsync(id);
            if (dispatch == null)
            {
                return NotFound();
            }
            return Ok(dispatch);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<SurveyDispatchDto>> Create(SurveyDispatchSaveDto dto)
        {
            try
            {
                var created = await _surveyDispatchService.CreateAndSendAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (SurveyValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _surveyDispatchService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}