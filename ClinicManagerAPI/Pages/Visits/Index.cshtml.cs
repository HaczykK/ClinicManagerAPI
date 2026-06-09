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
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IVisitService visitService, UserManager<ApplicationUser> userManager)
    {
        _visitService = visitService;
        _userManager = userManager;
    }

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
    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;

        var doctors = await _userManager.GetUsersInRoleAsync("Lekarz");
        Doctors = doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToList();

        var result = await _visitService.GetPagedAsync(PageNumber, pageSize, Date, Status, DoctorId);
        Visits = result.Items;
        TotalPages = result.TotalPages;
    }
}
