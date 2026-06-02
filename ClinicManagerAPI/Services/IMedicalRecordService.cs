using ClinicManagerAPI.DTOs.MedicalRecords;

namespace ClinicManagerAPI.Services
{
    public interface IMedicalRecordService
    {
        Task<IReadOnlyList<MedicalRecordDto>> GetByPatientIdAsync(int patientId);
        Task<MedicalRecordDto?> GetByIdAsync(int id);
        Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto);
        Task DeleteAsync(int id);
    }
}
