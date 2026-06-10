using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.DTOs.Reports;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly IPdfService _pdfService;
    private readonly IPatientService _patientService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(
        IPdfService pdfService,
        IPatientService patientService,
        UserManager<ApplicationUser> userManager)
    {
        _pdfService = pdfService;
        _patientService = patientService;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public CostReportFilter Filter { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? UpcomingVisitsDate { get; set; }

    public IReadOnlyList<PatientListDto> Patients { get; set; } = [];
    public IReadOnlyList<ApplicationUser> Doctors { get; set; } = [];

    public async Task OnGetAsync()
    {
        var patientsResult = await _patientService.GetAllAsync(1, 100);
        Patients = patientsResult.Items;

        var doctors = await _userManager.GetUsersInRoleAsync("Lekarz");
        Doctors = doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToList();

        UpcomingVisitsDate ??= DateTime.UtcNow.Date;
    }

    public async Task<IActionResult> OnPostDownloadCostReportAsync()
    {
        var pdf = await _pdfService.GenerateCostReportPdf(Filter);
        return File(pdf, "application/pdf", $"raport-kosztow-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnPostDownloadUpcomingVisitsAsync()
    {
        var date = UpcomingVisitsDate ?? DateTime.Today;
        var pdf = await _pdfService.GenerateUpcomingVisitsPdf(date);
        return File(pdf, "application/pdf", $"wizyty-{date:yyyyMMdd}.pdf");
    }
}
