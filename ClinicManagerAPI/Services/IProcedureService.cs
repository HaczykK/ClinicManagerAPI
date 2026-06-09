using ClinicManagerAPI.DTOs.Procedures;
using ClinicManagerAPI.DTOs.PrescribedMedications;

namespace ClinicManagerAPI.Services
{
    public interface IProcedureService
    {
        Task<IReadOnlyList<ProcedurePerformedDto>> GetByVisitIdAsync(int visitId);
        Task<ProcedurePerformedDto> CreateAsync(CreateProcedurePerformedDto dto);
        Task<ProcedurePerformedDto> UpdateAsync(int id, UpdateProcedurePerformedDto dto);
        Task DeleteAsync(int id);
        Task<PrescribedMedicationDto> AddMedicationAsync(int procedureId, CreatePrescribedMedicationDto dto);
        Task DeletePrescribedMedicationAsync(int id);
    }
}
