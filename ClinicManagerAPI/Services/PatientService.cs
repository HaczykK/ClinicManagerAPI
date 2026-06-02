using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientMapper _mapper;

        public PatientService(ApplicationDbContext context, PatientMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<PatientListDto>> GetAllAsync()
        {
            var patients = await _context.Patients
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            return _mapper.ToListDtos(patients);
        }

        public async Task<PatientDto?> GetByIdAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            return patient is null ? null : _mapper.ToDto(patient);
        }

        public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
        {
            await EnsurePeselIsUniqueAsync(dto.Pesel);

            var patient = _mapper.ToEntity(dto);
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(patient);
        }

        public async Task<PatientDto> UpdateAsync(int id, UpdatePatientDto dto)
        {
            var patient = await _context.Patients.FindAsync(id)
                ?? throw new KeyNotFoundException($"Pacjent o id {id} nie został znaleziony.");

            await EnsurePeselIsUniqueAsync(dto.Pesel, id);

            _mapper.ApplyUpdate(dto, patient);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(patient);
        }

        public async Task DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id)
                ?? throw new KeyNotFoundException($"Pacjent o id {id} nie został znaleziony.");

            if (patient.IsDeleted)
            {
                return;
            }

            patient.IsDeleted = true;
            patient.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<PatientListDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }

            var normalizedQuery = query.Trim().ToLower();

            var patients = await _context.Patients
                .Where(p =>
                    p.FirstName.ToLower().Contains(normalizedQuery) ||
                    p.LastName.ToLower().Contains(normalizedQuery) ||
                    p.Pesel.Contains(normalizedQuery))
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            return _mapper.ToListDtos(patients);
        }

        private async Task EnsurePeselIsUniqueAsync(string pesel, int? excludePatientId = null)
        {
            var query = _context.Patients.Where(p => p.Pesel == pesel);

            if (excludePatientId.HasValue)
            {
                query = query.Where(p => p.Id != excludePatientId.Value);
            }

            if (await query.AnyAsync())
            {
                throw new InvalidOperationException($"Pacjent z numerem PESEL {pesel} już istnieje.");
            }
        }
    }
}
