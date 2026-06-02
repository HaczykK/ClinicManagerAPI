using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.ClinicalNotes
{
    public class ClinicalNoteDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        [Required]
        public int VisitId { get; set; }
    }
}
