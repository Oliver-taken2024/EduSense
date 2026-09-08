using Microsoft.AspNetCore.Mvc;
using EduSense.BLL.Services;
using EduSense.Shared;
using System.ComponentModel.DataAnnotations;

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

        [HttpPost]
        public async Task<ActionResult<OrganisationDto>> Post([FromBody] OrganisationDto organisation)
        {
            // Skapa ny organisation
            try
            {
                var created = await _organisationService.CreateAsync(organisation);
                return CreatedAtAction(nameof(GetAll), created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrganisationDto>> Put(int id, [FromBody] OrganisationDto organisation)
        {
            if (id != organisation.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updated = await _organisationService.UpdateAsync(organisation);
                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
