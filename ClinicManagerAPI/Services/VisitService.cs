using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class VisitService : IVisitService
    {
        private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Zaplanowana",
            "W trakcie",
            "Zakończona",
            "Anulowana"
        };

        private readonly ApplicationDbContext _context;
        private readonly VisitMapper _mapper;

        public VisitService(ApplicationDbContext context, VisitMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<VisitListDto>> GetAllAsync()
        {
            var visits = await _context.Visits
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<VisitDto?> GetByIdAsync(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            return visit is null ? null : _mapper.ToDto(visit);
        }

        public async Task<IReadOnlyList<VisitListDto>> GetByPatientIdAsync(int patientId)
        {
            await EnsurePatientExistsAsync(patientId);

            var visits = await _context.Visits
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<IReadOnlyList<VisitListDto>> GetByDoctorIdAsync(string doctorName)
        {
            if (string.IsNullOrWhiteSpace(doctorName))
            {
                throw new ArgumentException("Nazwa lekarza jest wymagana.", nameof(doctorName));
            }

            var normalizedDoctorName = doctorName.Trim();

            var visits = await _context.Visits
                .Where(v => v.AssignedDoctor != null &&
                            v.AssignedDoctor.ToLower() == normalizedDoctorName.ToLower())
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<VisitDto> CreateAsync(CreateVisitDto dto)
        {
            await EnsurePatientExistsAsync(dto.PatientId);
            ValidateStatus(dto.Status);

            var visit = _mapper.ToEntity(dto);
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(visit);
        }

        public async Task<VisitDto> UpdateAsync(int id, UpdateVisitDto dto)
        {
            var visit = await _context.Visits.FindAsync(id)
                ?? throw new KeyNotFoundException($"Wizyta o id {id} nie została znaleziona.");

            await EnsurePatientExistsAsync(dto.PatientId);
            ValidateStatus(dto.Status);

            _mapper.ApplyUpdate(dto, visit);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(visit);
        }

        public async Task<VisitDto> UpdateStatusAsync(int id, string status)
        {
            var visit = await _context.Visits.FindAsync(id)
                ?? throw new KeyNotFoundException($"Wizyta o id {id} nie została znaleziona.");

            ValidateStatus(status);
            visit.Status = status.Trim();
            await _context.SaveChangesAsync();

            return _mapper.ToDto(visit);
        }

        public async Task<IReadOnlyList<VisitListDto>> GetTodayVisitsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var visits = await _context.Visits
                .Where(v => v.Date >= today && v.Date < tomorrow)
                .OrderBy(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        private async Task EnsurePatientExistsAsync(int patientId)
        {
            if (!await _context.Patients.AnyAsync(p => p.Id == patientId))
            {
                throw new KeyNotFoundException($"Pacjent o id {patientId} nie został znaleziony.");
            }
        }

        private static void ValidateStatus(string status)
        {
            if (!ValidStatuses.Contains(status.Trim()))
            {
                throw new ArgumentException(
                    $"Nieprawidłowy status wizyty. Dozwolone wartości: {string.Join(", ", ValidStatuses)}.",
                    nameof(status));
            }
        }
    }
}
