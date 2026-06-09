using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Visits;

public class IndexModel : PageModel
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IVisitService visitService, IPatientService patientService, UserManager<ApplicationUser> userManager)
    {
        _visitService = visitService;
        _patientService = patientService;
        _userManager = userManager;
    }

    [BindProperty]
    public CreateVisitDto CreateInput { get; set; } = new() { Date = DateTime.UtcNow.AddDays(1) };


    [BindProperty(SupportsGet = true)]
    public DateTime? Date { get; set; }

    [BindProperty(SupportsGet = true)]
    public VisitStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DoctorId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<VisitListDto> Visits { get; set; } = [];
    public IReadOnlyList<ApplicationUser> Doctors { get; set; } = [];
    public IReadOnlyList<ClinicManagerAPI.DTOs.Patients.PatientListDto> AllPatients { get; set; } = [];
    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;

        var doctors = await _userManager.GetUsersInRoleAsync("Lekarz");
        Doctors = doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToList();

        var result = await _visitService.GetPagedAsync(PageNumber, pageSize, Date, Status, DoctorId);
        Visits = result.Items;
        TotalPages = result.TotalPages;

        if (User.IsInRole("Rejestratorka") || User.IsInRole("Admin"))
        {
            var patientsResult = await _patientService.GetAllAsync(1, 1000); // Pobieramy maksymalnie dużo, by wyświetlić w dropdownie (w realnym appie użyto by select2/ajax)
            AllPatients = patientsResult.Items;
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            return RedirectToPage();
        }

        try
        {
            await _visitService.CreateAsync(CreateInput);
            TempData["SuccessMessage"] = "Wizyta została zaplanowana pomyślnie.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}
