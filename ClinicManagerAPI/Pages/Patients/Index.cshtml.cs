using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Patients;

public class IndexModel : PageModel
{
    private readonly IPatientService _patientService;

    public IndexModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<PatientListDto> Patients { get; set; } = [];
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool IsSearchMode { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;

        if (!string.IsNullOrWhiteSpace(Query))
        {
            IsSearchMode = true;
            Patients = await _patientService.SearchAsync(Query);
            TotalCount = Patients.Count;
            TotalPages = 1;
            return;
        }

        var result = await _patientService.GetAllAsync(PageNumber, pageSize);
        Patients = result.Items;
        TotalCount = result.TotalCount;
        TotalPages = result.TotalPages;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            await _patientService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Pacjent został usunięty.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Pacjent nie został znaleziony.";
        }

        return RedirectToPage();
    }
}
