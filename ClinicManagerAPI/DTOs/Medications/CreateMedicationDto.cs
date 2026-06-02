using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Medications
{
    public class CreateMedicationDto
    {
        [Required(ErrorMessage = "Nazwa leku jest wymagana.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Cena jednostkowa nie może być ujemna.")]
        public decimal UnitPrice { get; set; }
    }
}
