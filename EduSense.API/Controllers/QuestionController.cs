using Microsoft.AspNetCore.Mvc;
using EduSense.BLL.Services;
using EduSense.Shared;

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
        public async Task<ActionResult<IReadOnlyList<QuestionDto>>> GetAll()
        {
            var questions = await _questionService.GetAllAsync();
            return Ok(questions);
        }
    }
}
