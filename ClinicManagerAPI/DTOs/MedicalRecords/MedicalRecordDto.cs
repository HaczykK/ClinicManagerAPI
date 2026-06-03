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

        public string? Description { get; set; }

        public string? FileName { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
