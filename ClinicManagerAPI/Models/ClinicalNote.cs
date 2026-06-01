using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.Models
{
    public class ClinicalNote
    {
        public int Id { get; set; }

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Relationship with Visit
        public int VisitId { get; set; }
        public Visit? Visit { get; set; }
    }
}
