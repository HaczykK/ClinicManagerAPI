using ClinicManagerAPI.DTOs;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.Services
{
    public interface IVisitService
    {
        Task<PagedResult<VisitListDto>> GetPagedAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? date = null,
            VisitStatus? status = null,
            string? doctorId = null);
        Task<VisitDetailDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VisitListDto>> GetByPatientIdAsync(int patientId);
        Task<IReadOnlyList<VisitListDto>> GetByDoctorIdAsync(string doctorId);
        Task<VisitDto> CreateAsync(CreateVisitDto dto);
        Task<VisitDto> UpdateAsync(int id, UpdateVisitDto dto);
        Task<VisitDto> UpdateStatusAsync(int id, VisitStatus status);
        Task<VisitDto> AssignDoctorAsync(int id, string doctorId);
        Task DeleteAsync(int id);
        Task<IReadOnlyList<VisitListDto>> GetTodayVisitsAsync();
    }
}
