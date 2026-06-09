using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClinicManagerAPI.DTOs.ClinicalNotes;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers
{
    [Route("api")]
    public class ClinicalNotesController : JwtApiControllerBase
    {
        private readonly IClinicalNoteService _clinicalNoteService;

        public ClinicalNotesController(IClinicalNoteService clinicalNoteService)
        {
            _clinicalNoteService = clinicalNoteService;
        }

        [HttpGet("visits/{visitId:int}/clinical-notes")]
        [ProducesResponseType(typeof(IReadOnlyList<ClinicalNoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<ClinicalNoteDto>>> GetByVisitId(int visitId)
        {
            try
            {
                var notes = await _clinicalNoteService.GetByVisitIdAsync(visitId);
                return Ok(notes);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("visits/{visitId:int}/clinical-notes")]
        [Authorize(Roles = "Lekarz,Admin")]
        [ProducesResponseType(typeof(ClinicalNoteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClinicalNoteDto>> Create(int visitId, [FromBody] CreateClinicalNoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.VisitId = visitId;
            dto.Author = GetCurrentUserFullName();

            try
            {
                var created = await _clinicalNoteService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetByVisitId), new { visitId }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("clinical-notes/{id:int}")]
        [Authorize(Roles = "Lekarz,Admin")]
        [ProducesResponseType(typeof(ClinicalNoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClinicalNoteDto>> Update(int id, [FromBody] UpdateClinicalNoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _clinicalNoteService.GetByIdAsync(id);
            if (existing is null)
                return NotFound(new { message = $"Notatka kliniczna o id {id} nie została znaleziona." });

            if (!User.IsInRole("Admin") && existing.Author != GetCurrentUserFullName())
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Nie masz uprawnień do edycji tej notatki. Lekarz może edytować tylko własne notatki." });

            try
            {
                var updated = await _clinicalNoteService.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("clinical-notes/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _clinicalNoteService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private string GetCurrentUserFullName()
        {
            return User.FindFirstValue(ClaimTypes.Name) ?? "Nieznany";
        }
    }
}
