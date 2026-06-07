namespace ClinicManagerAPI.DTOs.Reports;

/// <summary>
/// Filtr do raportu kosztów świadczeń.
/// Wszystkie pola są opcjonalne — brak filtra oznacza raport za wszystkie wizyty.
/// </summary>
public class CostReportFilter
{
    public int? PatientId { get; set; }
    public string? DoctorId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
