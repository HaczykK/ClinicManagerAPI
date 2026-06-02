using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class UpdateVisitDto
    {
        [Required(ErrorMessage = "Data wizyty jest wymagana.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Status wizyty jest wymagany.")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AssignedDoctor { get; set; }

        [Required(ErrorMessage = "Identyfikator pacjenta jest wymagany.")]
        public int PatientId { get; set; }
    }
}
