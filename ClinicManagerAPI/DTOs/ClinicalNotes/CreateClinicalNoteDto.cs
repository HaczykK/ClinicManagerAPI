using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.ClinicalNotes
{
    public class CreateClinicalNoteDto
    {
        [Required(ErrorMessage = "Autor notatki jest wymagany.")]
        [MaxLength(200)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treść notatki jest wymagana.")]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Identyfikator wizyty jest wymagany.")]
        public int VisitId { get; set; }
    }
}
