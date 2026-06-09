using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.MedicalRecords
{
    public class UpdateMedicalRecordDto
    {
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
