using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public string DocumentScanUrl { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? FileName { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
