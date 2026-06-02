using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Patients
{
    public class PatientListDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string Pesel { get; set; } = string.Empty;
    }
}
