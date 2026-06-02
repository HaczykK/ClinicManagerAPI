using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.PrescribedMedications
{
    public class PrescribedMedicationDto
    {
        public int Id { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [MaxLength(200)]
        public string? MedicationName { get; set; }

        [Required]
        public int ProcedurePerformedId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od zera.")]
        public int Quantity { get; set; }
    }
}
