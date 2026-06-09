using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Patients
{
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "PESEL jest wymagany.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi mieć dokładnie 11 znaków.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL musi składać się z dokładnie 11 cyfr.")]
        public string Pesel { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? InsuranceNumber { get; set; }
    }
}
