using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.PrescribedMedications
{
    public class CreatePrescribedMedicationDto
    {
        [Required(ErrorMessage = "Identyfikator leku jest wymagany.")]
        public int MedicationId { get; set; }

        [Required(ErrorMessage = "Dawkowanie jest wymagane.")]
        [MaxLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od zera.")]
        public int Quantity { get; set; }
    }
}
