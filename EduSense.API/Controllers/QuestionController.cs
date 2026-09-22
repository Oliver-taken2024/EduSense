using EduSense.BLL.Services;
using EduSense.DAL.Models;
using EduSense.Shared;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuestionController(IQuestionService questionService, UserManager<ApplicationUser> userManager)
        {
            _questionService = questionService;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrAnalyst")]
        public async Task<ActionResult<IReadOnlyList<QuestionDto>>> GetAll()
        {
            var questions = await _questionService.GetAllAsync();

            // CreatedByUserId är Identity-Id:t, inte lämpligt att visa i UI - slå upp
            // e-post i en batch istället för en lookup per fråga.
            var userIds = questions.Select(q => q.CreatedByUserId).Distinct().ToList();
            var emailsByUserId = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? string.Empty);

            foreach (var question in questions)
            {
                question.CreatedByEmail = emailsByUserId.GetValueOrDefault(question.CreatedByUserId, question.CreatedByUserId);
            }

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
