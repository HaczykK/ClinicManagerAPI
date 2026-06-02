using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.MedicalRecords;
using ClinicManagerAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly ApplicationDbContext _context;
        private readonly MedicalRecordMapper _mapper;

        public MedicalRecordService(ApplicationDbContext context, MedicalRecordMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
        {
            await EnsurePatientExistsAsync(patientId);

            var records = await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return _mapper.ToDtos(records);
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            return record is null ? null : _mapper.ToDto(record);
        }

        public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
        {
            await EnsurePatientExistsAsync(dto.PatientId);

            var record = _mapper.ToEntity(dto);
            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(record);
        }

        public async Task DeleteAsync(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id)
                ?? throw new KeyNotFoundException($"Dokument kartoteki o id {id} nie został znaleziony.");

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
        }

        private async Task EnsurePatientExistsAsync(int patientId)
        {
            if (!await _context.Patients.AnyAsync(p => p.Id == patientId))
            {
                throw new KeyNotFoundException($"Pacjent o id {patientId} nie został znaleziony.");
            }
        }
    }
}
