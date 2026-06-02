using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagerAPI.DTOs;
using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IVisitService _visitService;

        public PatientsController(IPatientService patientService, IVisitService visitService)
        {
            _patientService = patientService;
            _visitService = visitService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<PatientListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<PatientListDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            var result = await _patientService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDto>> GetById(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient is null)
                return NotFound(new { message = $"Pacjent o id {id} nie został znaleziony." });

            return Ok(patient);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IReadOnlyList<PatientListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IReadOnlyList<PatientListDto>>> Search([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Parametr 'query' jest wymagany." });

            var results = await _patientService.SearchAsync(query);
            return Ok(results);
        }

        [HttpGet("{id:int}/visits")]
        [ProducesResponseType(typeof(IReadOnlyList<VisitListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<VisitListDto>>> GetPatientVisits(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient is null)
                return NotFound(new { message = $"Pacjent o id {id} nie został znaleziony." });

            var visits = await _visitService.GetByPatientIdAsync(id);
            return Ok(visits);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Rejestratorka")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PatientDto>> Create([FromBody] CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _patientService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Rejestratorka")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PatientDto>> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _patientService.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _patientService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
