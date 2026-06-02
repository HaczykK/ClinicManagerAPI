using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class UpdateVisitDto
    {
        [Required(ErrorMessage = "Data wizyty jest wymagana.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Status wizyty jest wymagany.")]
        public VisitStatus Status { get; set; }

        [MaxLength(450)]
        public string? AssignedDoctorId { get; set; }

        [Required(ErrorMessage = "Identyfikator pacjenta jest wymagany.")]
        public int PatientId { get; set; }
    }
}
