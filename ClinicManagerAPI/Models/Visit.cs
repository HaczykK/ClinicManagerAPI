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
        public VisitStatus Status { get; set; } = VisitStatus.Zaplanowana;

        public string? AssignedDoctorId { get; set; }
        public ApplicationUser? AssignedDoctor { get; set; }

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public List<ProcedurePerformed> ProceduresPerformed { get; set; } = new();
        public List<ClinicalNote> ClinicalNotes { get; set; } = new();
    }
}