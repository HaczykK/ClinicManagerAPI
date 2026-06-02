using ClinicManagerAPI.DTOs.Visits;

namespace ClinicManagerAPI.Services
{
    public interface IVisitService
    {
        Task<IReadOnlyList<VisitListDto>> GetAllAsync();
        Task<VisitDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VisitListDto>> GetByPatientIdAsync(int patientId);
        Task<IReadOnlyList<VisitListDto>> GetByDoctorIdAsync(string doctorName);
        Task<VisitDto> CreateAsync(CreateVisitDto dto);
        Task<VisitDto> UpdateAsync(int id, UpdateVisitDto dto);
        Task<VisitDto> UpdateStatusAsync(int id, string status);
        Task<IReadOnlyList<VisitListDto>> GetTodayVisitsAsync();
    }
}
