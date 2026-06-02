using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.Services
{
    public interface IVisitService
    {
        Task<IReadOnlyList<VisitListDto>> GetAllAsync();
        Task<VisitDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VisitListDto>> GetByPatientIdAsync(int patientId);
        Task<IReadOnlyList<VisitListDto>> GetByDoctorIdAsync(string doctorId);
        Task<VisitDto> CreateAsync(CreateVisitDto dto);
        Task<VisitDto> UpdateAsync(int id, UpdateVisitDto dto);
        Task<VisitDto> UpdateStatusAsync(int id, VisitStatus status);
        Task<IReadOnlyList<VisitListDto>> GetTodayVisitsAsync();
    }
}
