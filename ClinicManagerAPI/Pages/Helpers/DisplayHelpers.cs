using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.Pages.Helpers;

public static class DisplayHelpers
{
    public static string FormatVisitStatus(VisitStatus status) => status switch
    {
        VisitStatus.Zaplanowana => "Zaplanowana",
        VisitStatus.WTrakcie => "W trakcie",
        VisitStatus.Zakonczona => "Zakończona",
        VisitStatus.Anulowana => "Anulowana",
        _ => status.ToString()
    };

    public static string StatusBadgeClass(VisitStatus status) => status switch
    {
        VisitStatus.Zaplanowana => "bg-primary",
        VisitStatus.WTrakcie => "bg-warning text-dark",
        VisitStatus.Zakonczona => "bg-success",
        VisitStatus.Anulowana => "bg-secondary",
        _ => "bg-light text-dark"
    };
}
