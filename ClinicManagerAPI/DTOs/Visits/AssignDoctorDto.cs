using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class AssignDoctorDto
    {
        [Required(ErrorMessage = "Identyfikator lekarza jest wymagany.")]
        [MaxLength(450)]
        public string DoctorId { get; set; } = string.Empty;
    }
}
