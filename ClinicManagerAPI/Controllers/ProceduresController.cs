using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagerAPI.DTOs.Procedures;
using ClinicManagerAPI.DTOs.PrescribedMedications;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers
{
    [Route("api")]
    public class ProceduresController : JwtApiControllerBase
    {
        private readonly IProcedureService _procedureService;

        public ProceduresController(IProcedureService procedureService)
        {
            _procedureService = procedureService;
        }

        [HttpGet("visits/{visitId:int}/procedures")]
        [ProducesResponseType(typeof(IReadOnlyList<ProcedurePerformedDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<ProcedurePerformedDto>>> GetByVisitId(int visitId)
        {
            try
            {
                var procedures = await _procedureService.GetByVisitIdAsync(visitId);
                return Ok(procedures);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("visits/{visitId:int}/procedures")]
        [Authorize(Roles = "Lekarz,Admin")]
        [ProducesResponseType(typeof(ProcedurePerformedDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProcedurePerformedDto>> Create(int visitId, [FromBody] CreateProcedurePerformedDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.VisitId = visitId;

            try
            {
                var created = await _procedureService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetByVisitId), new { visitId }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("procedures/{id:int}")]
        [Authorize(Roles = "Lekarz,Admin")]
        [ProducesResponseType(typeof(ProcedurePerformedDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProcedurePerformedDto>> Update(int id, [FromBody] UpdateProcedurePerformedDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _procedureService.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("procedures/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _procedureService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("procedures/{procedureId:int}/medications")]
        [Authorize(Roles = "Lekarz,Admin")]
        [ProducesResponseType(typeof(PrescribedMedicationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PrescribedMedicationDto>> AddMedication(
            int procedureId,
            [FromBody] CreatePrescribedMedicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _procedureService.AddMedicationAsync(procedureId, dto);
                return Created($"api/prescribed-medications/{created.Id}", created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("prescribed-medications/{id:int}")]
        [Authorize(Roles = "Admin,Lekarz")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePrescribedMedication(int id)
        {
            try
            {
                await _procedureService.DeletePrescribedMedicationAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
