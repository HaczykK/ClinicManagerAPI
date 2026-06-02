using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.DTOs.PrescribedMedications;

namespace ClinicManagerAPI.DTOs.Procedures
{
    public class ProcedurePerformedDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Koszt świadczenia nie może być ujemny.")]
        public decimal ServiceCost { get; set; }

        [Required]
        public int VisitId { get; set; }

        public List<PrescribedMedicationDto> PrescribedMedications { get; set; } = new();
    }
}
