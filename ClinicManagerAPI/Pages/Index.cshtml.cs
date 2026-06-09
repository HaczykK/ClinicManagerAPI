using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClinicManagerAPI.Pages;

public class IndexModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly IVisitService _visitService;

    public IndexModel(IPatientService patientService, IVisitService visitService)
    {
        _patientService = patientService;
        _visitService = visitService;
    }

    public int PatientCount { get; set; }
    public int TodayVisitCount { get; set; }
    public IReadOnlyList<VisitListDto> RecentVisits { get; set; } = [];

    public async Task OnGetAsync()
    {
        var patients = await _patientService.GetAllAsync(1, 1);
        PatientCount = patients.TotalCount;

        var todayVisits = await _visitService.GetTodayVisitsAsync();
        TodayVisitCount = todayVisits.Count;

        var recent = await _visitService.GetPagedAsync(1, 5);
        RecentVisits = recent.Items;
    }
}
