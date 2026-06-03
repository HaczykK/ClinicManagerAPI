using System.ComponentModel.DataAnnotations;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.DTOs.Visits
{
    public class UpdateVisitStatusDto
    {
        [Required(ErrorMessage = "Status wizyty jest wymagany.")]
        public VisitStatus Status { get; set; }
    }
}
