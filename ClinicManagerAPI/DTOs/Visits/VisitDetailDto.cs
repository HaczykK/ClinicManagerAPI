using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.DTOs.ClinicalNotes;
using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.DTOs.Procedures;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class VisitDetailDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public VisitStatus Status { get; set; }

        [MaxLength(450)]
        public string? AssignedDoctorId { get; set; }

        [MaxLength(200)]
        public string? AssignedDoctorName { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public PatientDto Patient { get; set; } = null!;

        public IReadOnlyList<ProcedurePerformedDto> Procedures { get; set; } = [];

        public IReadOnlyList<ClinicalNoteDto> ClinicalNotes { get; set; } = [];
    }
}
