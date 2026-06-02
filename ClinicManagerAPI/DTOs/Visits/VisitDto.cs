using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class VisitDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AssignedDoctor { get; set; }

        [Required]
        public int PatientId { get; set; }
    }
}
