using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.ClinicalNotes
{
    public class UpdateClinicalNoteDto
    {
        [Required(ErrorMessage = "Treść notatki jest wymagana.")]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;
    }
}
