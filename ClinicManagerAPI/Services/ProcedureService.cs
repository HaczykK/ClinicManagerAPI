using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Procedures;
using ClinicManagerAPI.DTOs.PrescribedMedications;
using ClinicManagerAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class ProcedureService : IProcedureService
    {
        private readonly ApplicationDbContext _context;
        private readonly ProcedureMapper _procedureMapper;
        private readonly PrescribedMedicationMapper _prescribedMedicationMapper;

        public ProcedureService(
            ApplicationDbContext context,
            ProcedureMapper procedureMapper,
            PrescribedMedicationMapper prescribedMedicationMapper)
        {
            _context = context;
            _procedureMapper = procedureMapper;
            _prescribedMedicationMapper = prescribedMedicationMapper;
        }

        public async Task<IReadOnlyList<ProcedurePerformedDto>> GetByVisitIdAsync(int visitId)
        {
            await EnsureVisitExistsAsync(visitId);

            var procedures = await _context.ProceduresPerformed
                .Include(p => p.PrescribedMedications)
                    .ThenInclude(pm => pm.Medication)
                .Where(p => p.VisitId == visitId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var dtos = _procedureMapper.ToDtos(procedures);
            for (var i = 0; i < procedures.Count; i++)
            {
                dtos[i].PrescribedMedications = _prescribedMedicationMapper.ToDtos(procedures[i].PrescribedMedications);
            }

            return dtos;
        }

        public async Task<ProcedurePerformedDto> CreateAsync(CreateProcedurePerformedDto dto)
        {
            await EnsureVisitExistsAsync(dto.VisitId);

            var procedure = _procedureMapper.ToEntity(dto);
            _context.ProceduresPerformed.Add(procedure);
            await _context.SaveChangesAsync();

            return _procedureMapper.ToDto(procedure);
        }

        public async Task DeleteAsync(int id)
        {
            var procedure = await _context.ProceduresPerformed.FindAsync(id)
                ?? throw new KeyNotFoundException($"Procedura o id {id} nie została znaleziona.");

            _context.ProceduresPerformed.Remove(procedure);
            await _context.SaveChangesAsync();
        }

        public async Task<PrescribedMedicationDto> AddMedicationAsync(int procedureId, CreatePrescribedMedicationDto dto)
        {
            var procedure = await _context.ProceduresPerformed.FindAsync(procedureId)
                ?? throw new KeyNotFoundException($"Procedura o id {procedureId} nie została znaleziona.");

            var medication = await _context.Medications.FindAsync(dto.MedicationId)
                ?? throw new KeyNotFoundException($"Lek o id {dto.MedicationId} nie został znaleziony.");

            var prescribedMedication = _prescribedMedicationMapper.ToEntity(dto);
            prescribedMedication.ProcedurePerformedId = procedure.Id;

            _context.PrescribedMedications.Add(prescribedMedication);
            await _context.SaveChangesAsync();

            prescribedMedication.Medication = medication;
            return _prescribedMedicationMapper.ToDto(prescribedMedication);
        }

        public async Task DeletePrescribedMedicationAsync(int id)
        {
            var prescribedMedication = await _context.PrescribedMedications.FindAsync(id)
                ?? throw new KeyNotFoundException($"Przepisany lek o id {id} nie został znaleziony.");

            _context.PrescribedMedications.Remove(prescribedMedication);
            await _context.SaveChangesAsync();
        }

        private async Task EnsureVisitExistsAsync(int visitId)
        {
            if (!await _context.Visits.AnyAsync(v => v.Id == visitId))
            {
                throw new KeyNotFoundException($"Wizyta o id {visitId} nie została znaleziona.");
            }
        }
    }
}
