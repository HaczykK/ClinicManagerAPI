using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Procedures
{
    public class CreateProcedurePerformedDto
    {
        [Required(ErrorMessage = "Opis procedury jest wymagany.")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Koszt świadczenia nie może być ujemny.")]
        public decimal ServiceCost { get; set; }

        [Required(ErrorMessage = "Identyfikator wizyty jest wymagany.")]
        public int VisitId { get; set; }
    }
}
