using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.MedicalRecords
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string DocumentScanUrl { get; set; } = string.Empty;

        [Required]
        public int PatientId { get; set; }
    }
}
