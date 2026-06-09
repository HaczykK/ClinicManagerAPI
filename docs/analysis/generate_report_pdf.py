"""Generate raport-indeksy.pdf from analysis data."""

from pathlib import Path

try:
    from fpdf import FPDF
except ImportError:
    import subprocess
    import sys

    subprocess.check_call([sys.executable, "-m", "pip", "install", "fpdf2", "-q"])
    from fpdf import FPDF

ROOT = Path(__file__).resolve().parents[2]
ANALYSIS = Path(__file__).resolve().parent
OUTPUT = ROOT / "raport-indeksy.pdf"


def read_snippet(path: Path, max_lines: int = 12) -> str:
    if not path.exists():
        return "(brak pliku)"
    lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
    return "\n".join(lines[:max_lines])


class ReportPDF(FPDF):
    def header(self):
        self.set_font("Helvetica", "B", 14)
        self.cell(0, 10, "Raport: Indeksy nieklastrowane - ClinicManagerAPI", ln=True)
        self.ln(2)

    def section(self, title: str):
        self.set_font("Helvetica", "B", 11)
        self.multi_cell(0, 7, title)
        self.ln(1)

    def body_text(self, text: str):
        self.set_font("Helvetica", "", 9)
        self.multi_cell(0, 5, text)
        self.ln(2)

    def code_block(self, text: str):
        self.set_font("Courier", "", 7)
        self.set_fill_color(245, 245, 245)
        for line in text.splitlines():
            self.cell(0, 4, line[:120], ln=True, fill=True)
        self.ln(2)


def main():
    pdf = ReportPDF()
    pdf.set_auto_page_break(auto=True, margin=15)
    pdf.add_page()

    pdf.section("1. Cel")
    pdf.body_text(
        "Dodanie indeksow nieklastrowanych do kolumn czesto uzywanych w WHERE/JOIN "
        "w bazie ClinicManagerDB. Migracja: 20260609165030_AddNonClusteredIndexes."
    )

    pdf.section("2. Zidentyfikowane kolumny")
    pdf.body_text(
        "Patients.Pesel (UNIQUE) - wyszukiwanie pacjenta\n"
        "Patients.LastName - wyszukiwanie po nazwisku\n"
        "Visits.Date - filtrowanie wizyt po dacie\n"
        "Visits.Status - filtrowanie po statusie\n"
        "Visits.AssignedDoctorId, Visit.PatientId - juz istnialy (FK)"
    )

    pdf.section("3. Zapytanie 1 - PESEL")
    pdf.code_block(
        "SELECT Id, FirstName, LastName, Pesel\n"
        "FROM Patients\n"
        "WHERE Pesel = '85010112345' AND IsDeleted = 0;"
    )
    pdf.body_text(
        "PRZED: Clustered Index Scan (PK_Patients), logical reads = 3\n"
        "PO:    Index Seek (IX_Patients_Pesel) + Key Lookup, logical reads = 4\n"
        "Wniosek: Scan -> Seek - indeks PESEL dziala poprawnie."
    )
    pdf.body_text("Fragment planu PRZED:")
    pdf.code_block(read_snippet(ANALYSIS / "before-query1-pesel.txt", 11))
    pdf.body_text("Fragment planu PO:")
    pdf.code_block(read_snippet(ANALYSIS / "after-query1-pesel.txt", 13))

    pdf.section("4. Zapytanie 2 - wizyty na dzis")
    pdf.code_block(
        "SELECT Id, Date, Status, PatientId, AssignedDoctorId\n"
        "FROM Visits\n"
        "WHERE Date >= @dayStart AND Date < @dayEnd;"
    )
    pdf.body_text(
        "PRZED: Clustered Index Scan, logical reads = 3\n"
        "PO:    Clustered Index Scan, logical reads = 3 (tabela: 8 wierszy)\n"
        "Indeks IX_Visits_Date utworzony - optymalizator wybiera Scan przy malym zbiorze."
    )

    pdf.section("5. Utworzone indeksy")
    pdf.body_text(
        "IX_Patients_Pesel (unique), IX_Patients_LastName\n"
        "IX_Visits_Date, IX_Visits_Status"
    )

    pdf.section("6. Podsumowanie")
    pdf.body_text(
        "Najwieksza poprawa: zapytanie po PESEL (Scan -> Index Seek).\n"
        "Indeksy na Date/Status gotowe; przy wiekszej liczbie rekordow SQL Server\n"
        "przejdzie na Index Seek. Pelne logi w docs/analysis/."
    )

    pdf.output(str(OUTPUT))
    print(f"Generated: {OUTPUT}")


if __name__ == "__main__":
    main()
