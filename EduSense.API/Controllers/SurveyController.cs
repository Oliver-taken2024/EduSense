using Microsoft.AspNetCore.Mvc;
using EduSense.BLL.Services;
using EduSense.Shared;
using EduSense.BLL.Exceptions;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyService _surveyService;

        public SurveyController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SurveyDto>>> GetAll()
        {
            var surveys = await _surveyService.GetAllAsync();
            return Ok(surveys);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SurveyDto>> GetById(int id)
        {
            var survey = await _surveyService.GetByIdAsync(id);

            if (survey is null)
            {
                return NotFound();
            }

            return Ok(survey);
        }

        [HttpPost]
        public async Task<ActionResult<SurveyDto>> Create(SurveySaveDto dto)
        {
            try
            {
                var created = await _surveyService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (SurveyValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SurveyDto>> Update(int id, SurveySaveDto dto)
        {
            try
            {
                var updated = await _surveyService.UpdateAsync(id, dto);

                if (updated is null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (SurveyValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _surveyService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
