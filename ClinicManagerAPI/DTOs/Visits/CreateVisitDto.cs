using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class CreateVisitDto
    {
        [Required(ErrorMessage = "Data wizyty jest wymagana.")]
        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Zaplanowana";

        [MaxLength(200)]
        public string? AssignedDoctor { get; set; }

        [Required(ErrorMessage = "Identyfikator pacjenta jest wymagany.")]
        public int PatientId { get; set; }
    }
}
