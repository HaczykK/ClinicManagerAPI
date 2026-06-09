using ClinicManagerAPI.DTOs.MedicalRecords;

namespace ClinicManagerAPI.Services
{
    public interface IMedicalRecordService
    {
        Task<IReadOnlyList<MedicalRecordDto>> GetByPatientIdAsync(int patientId);
        Task<MedicalRecordDto?> GetByIdAsync(int id);
        Task<MedicalRecordDto> CreateAsync(int patientId, IFormFile file, string? description);
        Task<MedicalRecordDto> UpdateAsync(int id, UpdateMedicalRecordDto dto);
        Task DeleteAsync(int id);
    }
}
