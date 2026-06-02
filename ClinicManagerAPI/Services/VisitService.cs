using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Mappers;
using ClinicManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class VisitService : IVisitService
    {
        private readonly ApplicationDbContext _context;
        private readonly VisitMapper _mapper;

        public VisitService(ApplicationDbContext context, VisitMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<VisitListDto>> GetAllAsync()
        {
            var visits = await QueryWithDoctor()
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<VisitDto?> GetByIdAsync(int id)
        {
            var visit = await QueryWithDoctor().FirstOrDefaultAsync(v => v.Id == id);
            return visit is null ? null : _mapper.ToDto(visit);
        }

        public async Task<IReadOnlyList<VisitListDto>> GetByPatientIdAsync(int patientId)
        {
            await EnsurePatientExistsAsync(patientId);

            var visits = await QueryWithDoctor()
                .Where(v => v.PatientId == patientId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<IReadOnlyList<VisitListDto>> GetByDoctorIdAsync(string doctorId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
            {
                throw new ArgumentException("Identyfikator lekarza jest wymagany.", nameof(doctorId));
            }

            await EnsureDoctorExistsAsync(doctorId);

            var visits = await QueryWithDoctor()
                .Where(v => v.AssignedDoctorId == doctorId)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<VisitDto> CreateAsync(CreateVisitDto dto)
        {
            await EnsurePatientExistsAsync(dto.PatientId);
            await EnsureDoctorExistsAsync(dto.AssignedDoctorId);

            var visit = _mapper.ToEntity(dto);
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            await _context.Entry(visit).Reference(v => v.AssignedDoctor).LoadAsync();
            return _mapper.ToDto(visit);
        }

        public async Task<VisitDto> UpdateAsync(int id, UpdateVisitDto dto)
        {
            var visit = await _context.Visits.FindAsync(id)
                ?? throw new KeyNotFoundException($"Wizyta o id {id} nie została znaleziona.");

            await EnsurePatientExistsAsync(dto.PatientId);
            await EnsureDoctorExistsAsync(dto.AssignedDoctorId);

            _mapper.ApplyUpdate(dto, visit);
            await _context.SaveChangesAsync();

            await _context.Entry(visit).Reference(v => v.AssignedDoctor).LoadAsync();
            return _mapper.ToDto(visit);
        }

        public async Task<VisitDto> UpdateStatusAsync(int id, VisitStatus status)
        {
            var visit = await _context.Visits.FindAsync(id)
                ?? throw new KeyNotFoundException($"Wizyta o id {id} nie została znaleziona.");

            visit.Status = status;
            await _context.SaveChangesAsync();

            await _context.Entry(visit).Reference(v => v.AssignedDoctor).LoadAsync();
            return _mapper.ToDto(visit);
        }

        public async Task<IReadOnlyList<VisitListDto>> GetTodayVisitsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var visits = await QueryWithDoctor()
                .Where(v => v.Date >= today && v.Date < tomorrow)
                .OrderBy(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        private IQueryable<Visit> QueryWithDoctor() =>
            _context.Visits.Include(v => v.AssignedDoctor);

        private async Task EnsurePatientExistsAsync(int patientId)
        {
            if (!await _context.Patients.AnyAsync(p => p.Id == patientId))
            {
                throw new KeyNotFoundException($"Pacjent o id {patientId} nie został znaleziony.");
            }
        }

        private async Task EnsureDoctorExistsAsync(string? doctorId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
            {
                return;
            }

            if (!await _context.Users.AnyAsync(u => u.Id == doctorId))
            {
                throw new KeyNotFoundException($"Lekarz o id {doctorId} nie został znaleziony.");
            }
        }
    }
}
