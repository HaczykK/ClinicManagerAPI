using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Visits;

public class DetailsModel : PageModel
{
    private readonly IVisitService _visitService;
    private readonly IPdfService _pdfService;

    public DetailsModel(IVisitService visitService, IPdfService pdfService)
    {
        _visitService = visitService;
        _pdfService = pdfService;
    }

    public VisitDetailDto? Visit { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Visit = await _visitService.GetByIdAsync(id);
        if (Visit is null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnGetVisitCardPdfAsync(int id)
    {
        try
        {
            var pdf = await _pdfService.GenerateVisitCardPdf(id);
            return File(pdf, "application/pdf", $"karta-wizyty-{id}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnGetPrescriptionPdfAsync(int id)
    {
        if (!User.IsInRole("Lekarz") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            var pdf = await _pdfService.GeneratePrescriptionPdf(id);
            return File(pdf, "application/pdf", $"recepta-wizyta-{id}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
