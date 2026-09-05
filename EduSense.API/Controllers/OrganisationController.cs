using Microsoft.AspNetCore.Mvc;
using EduSense.BLL.Services;
using EduSense.Shared;

namespace EduSense.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;

        public OrganisationController(IOrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrganisationDto>>> GetAll()
        {
            var organisations = await _organisationService.GetAllAsync();
            return Ok(organisations);
        }
    }
}
