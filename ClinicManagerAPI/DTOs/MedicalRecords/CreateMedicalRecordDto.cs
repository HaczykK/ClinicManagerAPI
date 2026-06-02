using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.MedicalRecords
{
    public class CreateMedicalRecordDto
    {
        [Required(ErrorMessage = "Adres URL skanu dokumentu jest wymagany.")]
        [MaxLength(500)]
        public string DocumentScanUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Identyfikator pacjenta jest wymagany.")]
        public int PatientId { get; set; }
    }
}
