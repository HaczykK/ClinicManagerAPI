using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagerAPI.DTOs.Reports;
using ClinicManagerAPI.Services;

namespace ClinicManagerAPI.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IPdfService _pdfService;

    public ReportsController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Pobierz kartę wizyty w formacie PDF. Dostęp: wszyscy zalogowani.
    /// </summary>
    [HttpGet("visit-card/{visitId:int}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVisitCard(int visitId)
    {
        try
        {
            var pdf = await _pdfService.GenerateVisitCardPdf(visitId);
            return File(pdf, "application/pdf", $"karta-wizyty-{visitId}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Pobierz receptę w formacie PDF. Dostęp: Lekarz, Admin.
    /// </summary>
    [HttpGet("prescription/{visitId:int}")]
    [Authorize(Roles = "Lekarz,Admin")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrescription(int visitId)
    {
        try
        {
            var pdf = await _pdfService.GeneratePrescriptionPdf(visitId);
            return File(pdf, "application/pdf", $"recepta-wizyta-{visitId}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Pobierz raport kosztów świadczeń w formacie PDF. Dostęp: Admin, Rejestratorka.
    /// </summary>
    [HttpGet("costs")]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostReport([FromQuery] CostReportFilter filter)
    {
        var pdf = await _pdfService.GenerateCostReportPdf(filter);
        return File(pdf, "application/pdf", $"raport-kosztow-{DateTime.Now:yyyyMMdd}.pdf");
    }
}
