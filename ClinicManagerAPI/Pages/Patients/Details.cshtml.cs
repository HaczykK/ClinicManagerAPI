using ClinicManagerAPI.DTOs.MedicalRecords;
using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Patients;

public class DetailsModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly IVisitService _visitService;
    private readonly IMedicalRecordService _medicalRecordService;

    public DetailsModel(
        IPatientService patientService,
        IVisitService visitService,
        IMedicalRecordService medicalRecordService)
    {
        _patientService = patientService;
        _visitService = visitService;
        _medicalRecordService = medicalRecordService;
    }

    public PatientDto? Patient { get; set; }
    public IReadOnlyList<VisitListDto> Visits { get; set; } = [];
    public IReadOnlyList<MedicalRecordDto> MedicalRecords { get; set; } = [];

    [BindProperty]
    public string? UploadDescription { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Patient = await _patientService.GetByIdAsync(id);
        if (Patient is null)
        {
            return NotFound();
        }

        Visits = await _visitService.GetByPatientIdAsync(id);
        MedicalRecords = await _medicalRecordService.GetByPatientIdAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(int id, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Wybierz plik do przesłania.";
            return RedirectToPage(new { id });
        }

        if (!User.IsInRole("Admin") && !User.IsInRole("Rejestratorka") && !User.IsInRole("Lekarz"))
        {
            return Forbid();
        }

        try
        {
            await _medicalRecordService.CreateAsync(id, file, UploadDescription);
            TempData["SuccessMessage"] = "Dokument został dodany do kartoteki.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Pacjent nie został znaleziony.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteRecordAsync(int id, int recordId)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            await _medicalRecordService.DeleteAsync(recordId);
            TempData["SuccessMessage"] = "Dokument został usunięty.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Dokument nie został znaleziony.";
        }

        return RedirectToPage(new { id });
    }
}
