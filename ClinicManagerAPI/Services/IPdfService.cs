using ClinicManagerAPI.DTOs.Reports;

namespace ClinicManagerAPI.Services;

/// <summary>
/// Serwis generowania dokumentów PDF.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generuje kartę wizyty PDF z pełnymi danymi: pacjent, lekarz, procedury, leki, notatki.
    /// </summary>
    Task<byte[]> GenerateVisitCardPdf(int visitId);

    /// <summary>
    /// Generuje receptę PDF z listą przepisanych leków.
    /// </summary>
    Task<byte[]> GeneratePrescriptionPdf(int visitId);

    /// <summary>
    /// Generuje raport kosztów świadczeń z opcjonalnym filtrowaniem.
    /// </summary>
    Task<byte[]> GenerateCostReportPdf(CostReportFilter filter);

    /// <summary>
    /// Generuje raport PDF z wizytami zaplanowanymi na podany dzień.
    /// </summary>
    Task<byte[]> GenerateUpcomingVisitsPdf(DateTime date);
}
