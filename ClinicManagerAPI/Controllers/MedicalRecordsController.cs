using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagerAPI.DTOs.MedicalRecords;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet("patients/{patientId:int}/medical-records")]
        [ProducesResponseType(typeof(IReadOnlyList<MedicalRecordDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IReadOnlyList<MedicalRecordDto>>> GetByPatientId(int patientId)
        {
            try
            {
                var records = await _medicalRecordService.GetByPatientIdAsync(patientId);
                return Ok(records);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("medical-records/{id:int}")]
        [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalRecordDto>> GetById(int id)
        {
            var record = await _medicalRecordService.GetByIdAsync(id);
            if (record is null)
                return NotFound(new { message = $"Dokument kartoteki o id {id} nie został znaleziony." });

            return Ok(record);
        }

        [HttpPost("patients/{patientId:int}/medical-records")]
        [Authorize(Roles = "Admin,Rejestratorka,Lekarz")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MedicalRecordDto>> Create(
            int patientId,
            IFormFile file,
            [FromForm] string? description)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "Plik jest wymagany." });

            try
            {
                var created = await _medicalRecordService.CreateAsync(patientId, file, description);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

        [HttpDelete("medical-records/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _medicalRecordService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
