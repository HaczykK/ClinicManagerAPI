using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Elements.Table;
using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Reports;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.Services;

/// <summary>
/// Implementacja serwisu generowania dokumentów PDF przy użyciu QuestPDF.
/// </summary>
public class PdfService : IPdfService
{
    private readonly ApplicationDbContext _context;

    // Kolory firmowe
    private static readonly string PrimaryHex = "#1B5E7B";
    private static readonly string AccentHex = "#2196F3";
    private static readonly string LightBgHex = "#F5F9FC";
    private static readonly string HeaderTextHex = "#FFFFFF";
    private static readonly string BorderHex = "#D0DCE5";

    public PdfService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ───────────────────────────────────────────────────────
    // Karta wizyty
    // ───────────────────────────────────────────────────────
    public async Task<byte[]> GenerateVisitCardPdf(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.AssignedDoctor)
            .Include(v => v.ProceduresPerformed)
                .ThenInclude(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
            .Include(v => v.ClinicalNotes)
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit is null)
            throw new KeyNotFoundException($"Wizyta o id {visitId} nie została znaleziona.");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(35);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComposeHeader(c, "Karta wizyty"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(12);

                        // --- Informacje o wizycie ---
                        column.Item().Element(c => ComposeSectionTitle(c, "Informacje o wizycie"));
                        column.Item().Element(c =>
                        {
                            c.Background(LightBgHex).Padding(12).Column(infoCol =>
                            {
                                infoCol.Spacing(4);
                                infoCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("Nr wizyty: ").Bold();
                                        t.Span($"{visit.Id}");
                                    });
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("Data: ").Bold();
                                        t.Span(visit.Date.ToString("dd.MM.yyyy HH:mm"));
                                    });
                                });
                                infoCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("Status: ").Bold();
                                        t.Span(visit.Status.ToString());
                                    });
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("Lekarz: ").Bold();
                                        t.Span(visit.AssignedDoctor != null
                                            ? $"{visit.AssignedDoctor.FirstName} {visit.AssignedDoctor.LastName}"
                                            : "Nie przypisano");
                                    });
                                });
                            });
                        });

                        // --- Dane pacjenta ---
                        column.Item().Element(c => ComposeSectionTitle(c, "Dane pacjenta"));
                        column.Item().Element(c =>
                        {
                            c.Background(LightBgHex).Padding(12).Column(patCol =>
                            {
                                patCol.Spacing(4);
                                patCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("Imię i nazwisko: ").Bold();
                                        t.Span($"{visit.Patient?.FirstName} {visit.Patient?.LastName}");
                                    });
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span("PESEL: ").Bold();
                                        t.Span(visit.Patient?.Pesel ?? "—");
                                    });
                                });
                                patCol.Item().Text(t =>
                                {
                                    t.Span("Nr ubezpieczenia: ").Bold();
                                    t.Span(visit.Patient?.InsuranceNumber ?? "—");
                                });
                            });
                        });

                        // --- Procedury ---
                        if (visit.ProceduresPerformed.Any())
                        {
                            column.Item().Element(c => ComposeSectionTitle(c, "Wykonane procedury"));
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(30);
                                    cols.RelativeColumn(4);
                                    cols.RelativeColumn(1.5f);
                                });

                                table.Header(header =>
                                {
                                    ComposeTableHeaderCell(header.Cell(), "Lp.");
                                    ComposeTableHeaderCell(header.Cell(), "Opis procedury");
                                    ComposeTableHeaderCell(header.Cell(), "Koszt (zł)");
                                });

                                int idx = 1;
                                foreach (var proc in visit.ProceduresPerformed)
                                {
                                    ComposeTableCell(table.Cell(), idx.ToString());
                                    ComposeTableCell(table.Cell(), proc.Description);
                                    ComposeTableCell(table.Cell(), proc.ServiceCost.ToString("N2"));
                                    idx++;
                                }

                                // Suma
                                table.Cell().ColumnSpan(2).Border(1).BorderColor(BorderHex)
                                    .Background(LightBgHex).Padding(5)
                                    .AlignRight().Text("Suma:").Bold();
                                table.Cell().Border(1).BorderColor(BorderHex)
                                    .Background(LightBgHex).Padding(5)
                                    .Text(visit.ProceduresPerformed.Sum(p => p.ServiceCost).ToString("N2")).Bold();
                            });
                        }

                        // --- Przepisane leki ---
                        var allMeds = visit.ProceduresPerformed
                            .SelectMany(p => p.PrescribedMedications)
                            .ToList();

                        if (allMeds.Any())
                        {
                            column.Item().Element(c => ComposeSectionTitle(c, "Przepisane leki"));
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(30);
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    ComposeTableHeaderCell(header.Cell(), "Lp.");
                                    ComposeTableHeaderCell(header.Cell(), "Nazwa leku");
                                    ComposeTableHeaderCell(header.Cell(), "Dawkowanie");
                                    ComposeTableHeaderCell(header.Cell(), "Ilość");
                                });

                                int medIdx = 1;
                                foreach (var med in allMeds)
                                {
                                    ComposeTableCell(table.Cell(), medIdx.ToString());
                                    ComposeTableCell(table.Cell(), med.Medication?.Name ?? "—");
                                    ComposeTableCell(table.Cell(), med.Dosage);
                                    ComposeTableCell(table.Cell(), med.Quantity.ToString());
                                    medIdx++;
                                }
                            });
                        }

                        // --- Notatki kliniczne ---
                        if (visit.ClinicalNotes.Any())
                        {
                            column.Item().Element(c => ComposeSectionTitle(c, "Notatki kliniczne"));
                            foreach (var note in visit.ClinicalNotes.OrderBy(n => n.Timestamp))
                            {
                                column.Item().Element(c =>
                                {
                                    c.Background(LightBgHex).Padding(10).Column(noteCol =>
                                    {
                                        noteCol.Spacing(2);
                                        noteCol.Item().Text(t =>
                                        {
                                            t.Span($"{note.Timestamp:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                                            t.Span($"  —  {note.Author}").FontSize(8).Bold();
                                        });
                                        noteCol.Item().Text(note.Content);
                                    });
                                });
                            }
                        }
                    });
                });

                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    // ───────────────────────────────────────────────────────
    // Recepta
    // ───────────────────────────────────────────────────────
    public async Task<byte[]> GeneratePrescriptionPdf(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.AssignedDoctor)
            .Include(v => v.ProceduresPerformed)
                .ThenInclude(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit is null)
            throw new KeyNotFoundException($"Wizyta o id {visitId} nie została znaleziona.");

        var prescribedMeds = visit.ProceduresPerformed
            .SelectMany(p => p.PrescribedMedications)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(35);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComposeHeader(c, "Recepta"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(12);

                        // --- Dane pacjenta i lekarza ---
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c =>
                            {
                                c.Background(LightBgHex).Padding(12).Column(col =>
                                {
                                    col.Spacing(4);
                                    col.Item().Text("Pacjent").Bold().FontSize(11).FontColor(PrimaryHex);
                                    col.Item().Text($"{visit.Patient?.FirstName} {visit.Patient?.LastName}");
                                    col.Item().Text($"PESEL: {visit.Patient?.Pesel ?? "—"}");
                                    col.Item().Text($"Nr ubezp.: {visit.Patient?.InsuranceNumber ?? "—"}");
                                });
                            });
                            row.ConstantItem(15);
                            row.RelativeItem().Element(c =>
                            {
                                c.Background(LightBgHex).Padding(12).Column(col =>
                                {
                                    col.Spacing(4);
                                    col.Item().Text("Lekarz").Bold().FontSize(11).FontColor(PrimaryHex);
                                    col.Item().Text(visit.AssignedDoctor != null
                                        ? $"{visit.AssignedDoctor.FirstName} {visit.AssignedDoctor.LastName}"
                                        : "Nie przypisano");
                                    col.Item().Text($"Specjalizacja: {visit.AssignedDoctor?.Specialization ?? "—"}");
                                    col.Item().Text($"Data wizyty: {visit.Date:dd.MM.yyyy}");
                                });
                            });
                        });

                        column.Item().PaddingTop(5).Element(c => ComposeSectionTitle(c, "Przepisane leki"));

                        if (prescribedMeds.Any())
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(30);
                                    cols.RelativeColumn(3);
                                    cols.RelativeColumn(2);
                                    cols.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    ComposeTableHeaderCell(header.Cell(), "Lp.");
                                    ComposeTableHeaderCell(header.Cell(), "Nazwa leku");
                                    ComposeTableHeaderCell(header.Cell(), "Dawkowanie");
                                    ComposeTableHeaderCell(header.Cell(), "Ilość");
                                });

                                int idx = 1;
                                foreach (var med in prescribedMeds)
                                {
                                    ComposeTableCell(table.Cell(), idx.ToString());
                                    ComposeTableCell(table.Cell(), med.Medication?.Name ?? "—");
                                    ComposeTableCell(table.Cell(), med.Dosage);
                                    ComposeTableCell(table.Cell(), med.Quantity.ToString());
                                    idx++;
                                }
                            });
                        }
                        else
                        {
                            column.Item().Padding(10).Text("Brak przepisanych leków dla tej wizyty.")
                                .Italic().FontColor(Colors.Grey.Medium);
                        }

                        // Miejsce na podpisy
                        column.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().LineHorizontal(1).LineColor(BorderHex);
                                col.Item().PaddingTop(4).AlignCenter().Text("Podpis pacjenta")
                                    .FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(60);
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().LineHorizontal(1).LineColor(BorderHex);
                                col.Item().PaddingTop(4).AlignCenter().Text("Pieczątka i podpis lekarza")
                                    .FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });
                });

                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    // ───────────────────────────────────────────────────────
    // Raport kosztów
    // ───────────────────────────────────────────────────────
    public async Task<byte[]> GenerateCostReportPdf(CostReportFilter filter)
    {
        var query = _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.AssignedDoctor)
            .Include(v => v.ProceduresPerformed)
            .AsQueryable();

        if (filter.PatientId.HasValue)
            query = query.Where(v => v.PatientId == filter.PatientId.Value);

        if (!string.IsNullOrEmpty(filter.DoctorId))
            query = query.Where(v => v.AssignedDoctorId == filter.DoctorId);

        if (filter.DateFrom.HasValue)
            query = query.Where(v => v.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(v => v.Date <= filter.DateTo.Value);

        var visits = await query
            .OrderBy(v => v.Date)
            .ToListAsync();

        // Przygotuj etykietę zakresu dat
        var dateRangeLabel = (filter.DateFrom, filter.DateTo) switch
        {
            (not null, not null) => $"{filter.DateFrom.Value:dd.MM.yyyy} — {filter.DateTo.Value:dd.MM.yyyy}",
            (not null, null) => $"od {filter.DateFrom.Value:dd.MM.yyyy}",
            (null, not null) => $"do {filter.DateTo.Value:dd.MM.yyyy}",
            _ => "Cały okres"
        };

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginHorizontal(35);
                page.MarginVertical(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComposeHeader(c, "Raport kosztów świadczeń"));

                page.Content().Element(content =>
                {
                    content.PaddingVertical(8).Column(column =>
                    {
                        column.Spacing(10);

                        // Filtry zastosowane
                        column.Item().Background(LightBgHex).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Okres: ").Bold();
                                t.Span(dateRangeLabel);
                            });
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Liczba wizyt: ").Bold();
                                t.Span(visits.Count.ToString());
                            });
                        });

                        // Tabela
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(30);   // Lp.
                                cols.RelativeColumn(1.2f); // Data
                                cols.RelativeColumn(2);    // Pacjent
                                cols.RelativeColumn(2);    // Lekarz
                                cols.RelativeColumn(1);    // Status
                                cols.RelativeColumn(1);    // Liczba proc.
                                cols.RelativeColumn(1.2f); // Koszt
                            });

                            table.Header(header =>
                            {
                                ComposeTableHeaderCell(header.Cell(), "Lp.");
                                ComposeTableHeaderCell(header.Cell(), "Data");
                                ComposeTableHeaderCell(header.Cell(), "Pacjent");
                                ComposeTableHeaderCell(header.Cell(), "Lekarz");
                                ComposeTableHeaderCell(header.Cell(), "Status");
                                ComposeTableHeaderCell(header.Cell(), "Procedury");
                                ComposeTableHeaderCell(header.Cell(), "Koszt (zł)");
                            });

                            int idx = 1;
                            foreach (var v in visits)
                            {
                                var cost = v.ProceduresPerformed.Sum(p => p.ServiceCost);
                                ComposeTableCell(table.Cell(), idx.ToString());
                                ComposeTableCell(table.Cell(), v.Date.ToString("dd.MM.yyyy"));
                                ComposeTableCell(table.Cell(), v.Patient != null
                                    ? $"{v.Patient.FirstName} {v.Patient.LastName}"
                                    : "—");
                                ComposeTableCell(table.Cell(), v.AssignedDoctor != null
                                    ? $"{v.AssignedDoctor.FirstName} {v.AssignedDoctor.LastName}"
                                    : "—");
                                ComposeTableCell(table.Cell(), v.Status.ToString());
                                ComposeTableCell(table.Cell(), v.ProceduresPerformed.Count.ToString());
                                ComposeTableCell(table.Cell(), cost.ToString("N2"));
                                idx++;
                            }

                            // Suma
                            var totalCost = visits.Sum(v => v.ProceduresPerformed.Sum(p => p.ServiceCost));
                            table.Cell().ColumnSpan(6).Border(1).BorderColor(BorderHex)
                                .Background(LightBgHex).Padding(5)
                                .AlignRight().Text("RAZEM:").Bold().FontSize(10);
                            table.Cell().Border(1).BorderColor(BorderHex)
                                .Background(LightBgHex).Padding(5)
                                .Text(totalCost.ToString("N2")).Bold().FontSize(10);
                        });
                    });
                });

                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    // ───────────────────────────────────────────────────────
    // Wspólne elementy layoutu
    // ───────────────────────────────────────────────────────

    private static void ComposeHeader(IContainer container, string title)
    {
        container.Column(column =>
        {
            column.Item().Background(PrimaryHex).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Przychodnia ClinicManager")
                        .FontSize(16).Bold().FontColor(HeaderTextHex);
                    col.Item().Text("System zarządzania przychodnią")
                        .FontSize(9).FontColor(HeaderTextHex).Light();
                });
                row.RelativeItem().AlignRight().AlignBottom()
                    .Text(title)
                    .FontSize(14).Bold().FontColor(HeaderTextHex);
            });

            column.Item().LineHorizontal(3).LineColor(AccentHex);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(BorderHex);
            col.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Wygenerowano: ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Medium);
                });
                row.RelativeItem().AlignRight().Text(t =>
                {
                    t.Span("Strona ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private static void ComposeSectionTitle(IContainer container, string title)
    {
        container.Column(col =>
        {
            col.Item().Text(title).FontSize(12).Bold().FontColor(PrimaryHex);
            col.Item().PaddingTop(2).LineHorizontal(1).LineColor(AccentHex);
        });
    }

    private static void ComposeTableHeaderCell(ITableCellContainer cell, string text)
    {
        cell.Background(PrimaryHex).Padding(5)
            .Text(text).FontSize(9).Bold().FontColor(HeaderTextHex);
    }

    private static void ComposeTableCell(ITableCellContainer cell, string text)
    {
        cell.Border(1).BorderColor(BorderHex).Padding(5)
            .Text(text).FontSize(9);
    }
}
