using ClinicManagerAPI.DTOs.Medications;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Medications;

public class IndexModel : PageModel
{
    private readonly IMedicationService _medicationService;

    public IndexModel(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Name { get; set; }

    public IReadOnlyList<MedicationDto> Medications { get; set; } = [];

    [BindProperty]
    public CreateMedicationDto CreateInput { get; set; } = new();

    [BindProperty]
    public UpdateMedicationDto EditInput { get; set; } = new();

    [BindProperty]
    public int EditId { get; set; }

    public async Task OnGetAsync()
    {
        Medications = await _medicationService.GetAllAsync(Name);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        await _medicationService.CreateAsync(CreateInput);
        TempData["SuccessMessage"] = "Lek został dodany.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            await _medicationService.UpdateAsync(EditId, EditInput);
            TempData["SuccessMessage"] = "Lek został zaktualizowany.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Lek nie został znaleziony.";
        }

        return RedirectToPage(new { name = Name });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            await _medicationService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Lek został usunięty.";
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "Lek nie został znaleziony.";
        }

        return RedirectToPage(new { name = Name });
    }
}
