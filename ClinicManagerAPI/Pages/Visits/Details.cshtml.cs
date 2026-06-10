using System.Globalization;
using System.Security.Claims;
using ClinicManagerAPI.DTOs.ClinicalNotes;
using ClinicManagerAPI.DTOs.Medications;
using ClinicManagerAPI.DTOs.PrescribedMedications;
using ClinicManagerAPI.DTOs.Procedures;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Visits;

public class DetailsModel : PageModel
{
    private readonly IVisitService _visitService;
    private readonly IPdfService _pdfService;
    private readonly IClinicalNoteService _clinicalNoteService;
    private readonly IProcedureService _procedureService;
    private readonly IMedicationService _medicationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(
        IVisitService visitService,
        IPdfService pdfService,
        IClinicalNoteService clinicalNoteService,
        IProcedureService procedureService,
        IMedicationService medicationService,
        UserManager<ApplicationUser> userManager)
    {
        _visitService = visitService;
        _pdfService = pdfService;
        _clinicalNoteService = clinicalNoteService;
        _procedureService = procedureService;
        _medicationService = medicationService;
        _userManager = userManager;
    }

    public VisitDetailDto? Visit { get; set; }

    /// <summary>
    /// Lista leków z bazy do dropdownów.
    /// </summary>
    public IReadOnlyList<MedicationDto> AllMedications { get; set; } = [];

    /// <summary>
    /// Czy bieżący użytkownik może edytować tę wizytę (Lekarz prowadzący lub Admin).
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Czy bieżący użytkownik może anulować tę wizytę (Admin, Lekarz prowadzący lub Rejestratorka).
    /// </summary>
    public bool CanCancel { get; set; }

    // ── Binding properties dla formularzy ──

    [BindProperty]
    public string? NoteContent { get; set; }

    [BindProperty]
    public string? ProcedureDescription { get; set; }

    [BindProperty]
    public string? ProcedureServiceCost { get; set; }

    [BindProperty]
    public int MedProcedureId { get; set; }

    [BindProperty]
    public int MedMedicationId { get; set; }

    [BindProperty]
    public string? MedDosage { get; set; }

    [BindProperty]
    public int MedQuantity { get; set; }

    // ── GET ──

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Visit = await _visitService.GetByIdAsync(id);
        if (Visit is null)
        {
            return NotFound();
        }

        await LoadSupportingDataAsync();
        return Page();
    }

    // ── PDF handlers (istniejące) ──

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

    // ── Zmiana statusu wizyty ──

    public async Task<IActionResult> OnPostChangeStatusAsync(int id, VisitStatus newStatus)
    {
        bool isRejestratorka = User.IsInRole("Rejestratorka");
        
        // Rejestratorka może tylko anulować wizytę
        if (isRejestratorka && newStatus != VisitStatus.Anulowana)
        {
            return Forbid();
        }

        // Jeśli to nie jest Rejestratorka, wymagamy roli Lekarza lub Admina
        if (!isRejestratorka && !IsLekarzOrAdmin())
        {
            return Forbid();
        }

        try
        {
            await _visitService.UpdateStatusAsync(id, newStatus);
            TempData["SuccessMessage"] = $"Status wizyty zmieniony na: {FormatStatus(newStatus)}.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Wizyta nie została znaleziona.";
        }

        return RedirectToPage(new { id });
    }

    // ── Dodawanie notatki klinicznej ──

    public async Task<IActionResult> OnPostAddNoteAsync(int id)
    {
        if (!IsLekarzOrAdmin())
            return Forbid();

        if (string.IsNullOrWhiteSpace(NoteContent))
        {
            TempData["ErrorMessage"] = "Treść notatki jest wymagana.";
            return RedirectToPage(new { id });
        }

        try
        {
            var dto = new CreateClinicalNoteDto
            {
                VisitId = id,
                Content = NoteContent,
                Author = GetCurrentUserFullName()
            };
            await _clinicalNoteService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Notatka kliniczna została dodana.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Wizyta nie została znaleziona.";
        }

        return RedirectToPage(new { id });
    }

    // ── Dodawanie procedury ──

    public async Task<IActionResult> OnPostAddProcedureAsync(int id)
    {
        if (!IsLekarzOrAdmin())
            return Forbid();

        if (string.IsNullOrWhiteSpace(ProcedureDescription))
        {
            TempData["ErrorMessage"] = "Opis procedury jest wymagany.";
            return RedirectToPage(new { id });
        }

        if (!TryParseDecimal(ProcedureServiceCost, out var cost) || cost < 0)
        {
            TempData["ErrorMessage"] = "Nieprawidłowy format kosztu (użyj kropki lub przecinka).";
            return RedirectToPage(new { id });
        }

        try
        {
            var dto = new CreateProcedurePerformedDto
            {
                VisitId = id,
                Description = ProcedureDescription,
                ServiceCost = cost
            };
            await _procedureService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Procedura została dodana.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Wizyta nie została znaleziona.";
        }

        return RedirectToPage(new { id });
    }

    // ── Przepisywanie leku do procedury ──

    public async Task<IActionResult> OnPostAddMedicationAsync(int id)
    {
        if (!IsLekarzOrAdmin())
            return Forbid();

        if (string.IsNullOrWhiteSpace(MedDosage))
        {
            TempData["ErrorMessage"] = "Dawkowanie jest wymagane.";
            return RedirectToPage(new { id });
        }

        if (MedQuantity < 1)
        {
            TempData["ErrorMessage"] = "Ilość musi być większa od zera.";
            return RedirectToPage(new { id });
        }

        try
        {
            var dto = new CreatePrescribedMedicationDto
            {
                MedicationId = MedMedicationId,
                Dosage = MedDosage,
                Quantity = MedQuantity
            };
            await _procedureService.AddMedicationAsync(MedProcedureId, dto);
            TempData["SuccessMessage"] = "Lek został przepisany.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    // ── Metody pomocnicze ──

    private async Task LoadSupportingDataAsync()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        CanEdit = User.IsInRole("Admin")
                  || (User.IsInRole("Lekarz") && Visit?.AssignedDoctorId == currentUserId);

        CanCancel = CanEdit || User.IsInRole("Rejestratorka");

        if (CanEdit)
        {
            AllMedications = await _medicationService.GetAllAsync();
        }
    }

    private bool IsLekarzOrAdmin()
    {
        return User.IsInRole("Lekarz") || User.IsInRole("Admin");
    }

    private string GetCurrentUserFullName()
    {
        return User.FindFirstValue(ClaimTypes.Name) ?? "Nieznany";
    }

    private static bool TryParseDecimal(string? input, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var normalized = input.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private static string FormatStatus(VisitStatus status) => status switch
    {
        VisitStatus.Zaplanowana => "Zaplanowana",
        VisitStatus.WTrakcie => "W trakcie",
        VisitStatus.Zakonczona => "Zakończona",
        VisitStatus.Anulowana => "Anulowana",
        _ => status.ToString()
    };
}
