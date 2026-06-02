using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Medications
{
    public class MedicationDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Cena jednostkowa nie może być ujemna.")]
        public decimal UnitPrice { get; set; }
    }
}
