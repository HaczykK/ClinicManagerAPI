using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class VisitListDto
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

        public int PatientId { get; set; }
    }
}
