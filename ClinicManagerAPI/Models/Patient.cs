using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "PESEL jest wymagany.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 znaków.")]
        public string Pesel { get; set; } = string.Empty;

        public string? InsuranceNumber { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Visit> Visits { get; set; } = new();

        // Lista dokumentacji medycznej pacjenta
        public List<MedicalRecord> MedicalRecords { get; set; } = new();
    }
}