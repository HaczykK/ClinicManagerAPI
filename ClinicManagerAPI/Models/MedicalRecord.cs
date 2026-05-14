using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.Models
{
    public class MedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public string DocumentScanUrl { get; set; } = string.Empty;

        // Relationship with Patient
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
