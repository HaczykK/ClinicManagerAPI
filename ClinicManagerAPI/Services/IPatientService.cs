using ClinicManagerAPI.DTOs;
using ClinicManagerAPI.DTOs.Patients;

namespace ClinicManagerAPI.Services
{
    public interface IPatientService
    {
        Task<PagedResult<PatientListDto>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<PatientDto?> GetByIdAsync(int id);
        Task<PatientDto> CreateAsync(CreatePatientDto dto);
        Task<PatientDto> UpdateAsync(int id, UpdatePatientDto dto);
        Task DeleteAsync(int id);
        Task<IReadOnlyList<PatientListDto>> SearchAsync(string query);
    }
}
