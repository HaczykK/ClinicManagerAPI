using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class CreateVisitDto
    {
        [Required(ErrorMessage = "Data wizyty jest wymagana.")]
        public DateTime Date { get; set; }

        public VisitStatus Status { get; set; } = VisitStatus.Zaplanowana;

        [MaxLength(450)]
        public string? AssignedDoctorId { get; set; }

        [Required(ErrorMessage = "Identyfikator pacjenta jest wymagany.")]
        public int PatientId { get; set; }
    }
}
