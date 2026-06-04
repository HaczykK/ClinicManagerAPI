using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagerAPI.DTOs.Medications;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationsController : ControllerBase
    {
        private readonly IMedicationService _medicationService;

        public MedicationsController(IMedicationService medicationService)
        {
            _medicationService = medicationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<MedicationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<MedicationDto>>> GetAll([FromQuery] string? name)
        {
            var medications = await _medicationService.GetAllAsync(name);
            return Ok(medications);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicationDto>> GetById(int id)
        {
            var medication = await _medicationService.GetByIdAsync(id);
            if (medication is null)
                return NotFound(new { message = $"Lek o id {id} nie został znaleziony." });

            return Ok(medication);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Rejestratorka")]
        [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MedicationDto>> Create([FromBody] CreateMedicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _medicationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Rejestratorka")]
        [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicationDto>> Update(int id, [FromBody] UpdateMedicationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _medicationService.UpdateAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                await _medicationService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
