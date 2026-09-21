using EduSense.BLL.Services;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrAnalyst")]
        public async Task<ActionResult<IReadOnlyList<QuestionDto>>> GetAll()
        {
            var questions = await _questionService.GetAllAsync();
            return Ok(questions);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<QuestionDto>> Post([FromBody] QuestionDto question)
        {
            // Skapa ny fråga. CreatedByUserId sätts alltid från den inloggade användarens
            // claims - aldrig från vad klienten skickade in i request-bodyn (samma mönster
            // som SurveyDispatchController använder för SentByUserId).
            question.CreatedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            try
            {
                var created = await _questionService.CreateAsync(question);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Put(int id, [FromBody] QuestionDto question)
        {
            // Uppdatera befintlig fråga
            try
            {
                var updated = await _questionService.UpdateAsync(id, question);
                if (updated is null)
                {
                    return NotFound();
                }
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            // Ta bort befintlig fråga
            var deleted = await _questionService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }   
    }
}
