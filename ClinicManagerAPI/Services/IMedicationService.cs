using ClinicManagerAPI.DTOs.Medications;

namespace ClinicManagerAPI.Services
{
    public interface IMedicationService
    {
        Task<IReadOnlyList<MedicationDto>> GetAllAsync(string? name = null);
        Task<MedicationDto?> GetByIdAsync(int id);
        Task<MedicationDto> CreateAsync(CreateMedicationDto dto);
        Task<MedicationDto> UpdateAsync(int id, UpdateMedicationDto dto);
        Task DeleteAsync(int id);
    }
}
