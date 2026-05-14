using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.Models
{
    public class Visit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Data wizyty jest wymagana.")]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = "Zaplanowana"; // Np. Zaplanowana, W trakcie, Zakończona, Anulowana

        public string? AssignedDoctor { get; set; }

        // Powiązanie z Pacjentem (Klucz obcy w bazie danych)
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        // Powiązane procedury i notatki
        public List<ProcedurePerformed> ProceduresPerformed { get; set; } = new List<ProcedurePerformed>();
        public List<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    }
}