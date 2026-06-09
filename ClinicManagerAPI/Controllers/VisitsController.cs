using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagerAPI.DTOs;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers
{
    [Route("api/visits")]
    public class VisitsController : JwtApiControllerBase
    {
        private readonly IVisitService _visitService;

        public VisitsController(IVisitService visitService)
        {
            _visitService = visitService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<VisitListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<VisitListDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? date = null,
            [FromQuery] VisitStatus? status = null,
            [FromQuery] string? doctorId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            var result = await _visitService.GetPagedAsync(page, pageSize, date, status, doctorId);
            return Ok(result);
        }

        [HttpGet("today")]
        [ProducesResponseType(typeof(IReadOnlyList<VisitListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<VisitListDto>>> GetToday()
        {
            var visits = await _visitService.GetTodayVisitsAsync();
            return Ok(visits);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IReadOnlyList<ActiveVisitDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ActiveVisitDto>>> GetActive()
        {
            var visits = await _visitService.GetActiveVisitsAsync();
            return Ok(visits);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VisitDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitDetailDto>> GetById(int id)
        {
            var visit = await _visitService.GetByIdAsync(id);
            if (visit is null)
                return NotFound(new { message = $"Wizyta o id {id} nie została znaleziona." });

            return Ok(visit);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitDto>> Create([FromBody] CreateVisitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _visitService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Rejestratorka")]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitDto>> Update(int id, [FromBody] UpdateVisitDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _visitService.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitDto>> UpdateStatus(int id, [FromBody] UpdateVisitStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _visitService.UpdateStatusAsync(id, dto.Status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:int}/assign-doctor")]
        [Authorize(Roles = "Admin,Rejestratorka")]
        [ProducesResponseType(typeof(VisitDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VisitDto>> AssignDoctor(int id, [FromBody] AssignDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _visitService.AssignDoctorAsync(id, dto.DoctorId);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _visitService.DeleteAsync(id);
                return NoContent();
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
    }
}
