using ClinicManagerAPI.DTOs.ClinicalNotes;

namespace ClinicManagerAPI.Services
{
    public interface IClinicalNoteService
    {
        Task<IReadOnlyList<ClinicalNoteDto>> GetByVisitIdAsync(int visitId);
        Task<ClinicalNoteDto> CreateAsync(CreateClinicalNoteDto dto);
        Task<ClinicalNoteDto> UpdateAsync(int id, UpdateClinicalNoteDto dto);
        Task DeleteAsync(int id);
    }
}
