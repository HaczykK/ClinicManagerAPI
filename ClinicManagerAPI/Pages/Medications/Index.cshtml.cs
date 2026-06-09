using ClinicManagerAPI.DTOs.Medications;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

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

    public class MedicationInputModel
    {
        [Required(ErrorMessage = "Nazwa leku jest wymagana.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cena jednostkowa jest wymagana.")]
        public string UnitPrice { get; set; } = string.Empty;
    }

    [BindProperty]
    public MedicationInputModel CreateInput { get; set; } = new();

    [BindProperty]
    public MedicationInputModel EditInput { get; set; } = new();

    [BindProperty]
    public int EditId { get; set; }

    public async Task OnGetAsync()
    {
        Medications = await _medicationService.GetAllAsync(Name);
    }

    private bool TryParsePrice(string input, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;
        
        string normalized = input.Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        // Usunięcie błędów walidacji dla formularza edycji, aby nie blokowały dodawania
        var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("CreateInput")).ToList();
        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        if (!TryParsePrice(CreateInput.UnitPrice, out decimal parsedPrice))
        {
            ModelState.AddModelError("CreateInput.UnitPrice", "Nieprawidłowy format ceny (użyj kropki lub przecinka).");
            await OnGetAsync();
            return Page();
        }

        if (parsedPrice < 0)
        {
            ModelState.AddModelError("CreateInput.UnitPrice", "Cena nie może być ujemna.");
            await OnGetAsync();
            return Page();
        }

        var dto = new CreateMedicationDto
        {
            Name = CreateInput.Name,
            UnitPrice = parsedPrice
        };

        await _medicationService.CreateAsync(dto);
        TempData["SuccessMessage"] = "Lek został dodany.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        // Usunięcie błędów walidacji dla formularza dodawania, aby nie blokowały edycji
        var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("EditInput") && k != "EditId").ToList();
        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        if (!TryParsePrice(EditInput.UnitPrice, out decimal parsedPrice))
        {
            ModelState.AddModelError("EditInput.UnitPrice", "Nieprawidłowy format ceny (użyj kropki lub przecinka).");
            await OnGetAsync();
            return Page();
        }

        if (parsedPrice < 0)
        {
            ModelState.AddModelError("EditInput.UnitPrice", "Cena nie może być ujemna.");
            await OnGetAsync();
            return Page();
        }

        var dto = new UpdateMedicationDto
        {
            Name = EditInput.Name,
            UnitPrice = parsedPrice
        };

        try
        {
            await _medicationService.UpdateAsync(EditId, dto);
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
