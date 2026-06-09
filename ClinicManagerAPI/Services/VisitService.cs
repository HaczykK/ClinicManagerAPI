using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs;
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
        private readonly PatientMapper _patientMapper;
        private readonly ClinicalNoteMapper _clinicalNoteMapper;
        private readonly ProcedureMapper _procedureMapper;
        private readonly PrescribedMedicationMapper _prescribedMedicationMapper;

        public VisitService(
            ApplicationDbContext context,
            VisitMapper mapper,
            PatientMapper patientMapper,
            ClinicalNoteMapper clinicalNoteMapper,
            ProcedureMapper procedureMapper,
            PrescribedMedicationMapper prescribedMedicationMapper)
        {
            _context = context;
            _mapper = mapper;
            _patientMapper = patientMapper;
            _clinicalNoteMapper = clinicalNoteMapper;
            _procedureMapper = procedureMapper;
            _prescribedMedicationMapper = prescribedMedicationMapper;
        }

        public async Task<PagedResult<VisitListDto>> GetPagedAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? date = null,
            VisitStatus? status = null,
            string? doctorId = null)
        {
            var query = QueryWithDoctor();

            if (date.HasValue)
            {
                var dayStart = date.Value.Date;
                var dayEnd = dayStart.AddDays(1);
                query = query.Where(v => v.Date >= dayStart && v.Date < dayEnd);
            }

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(doctorId))
            {
                query = query.Where(v => v.AssignedDoctorId == doctorId);
            }

            query = query.OrderByDescending(v => v.Date);

            var totalCount = await query.CountAsync();

            var visits = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<VisitListDto>
            {
                Items = _mapper.ToListDtos(visits),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<VisitDetailDto?> GetByIdAsync(int id)
        {
            var visit = await QueryWithDetails().FirstOrDefaultAsync(v => v.Id == id);
            return visit is null ? null : ToDetailDto(visit);
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

        public async Task<VisitDto> AssignDoctorAsync(int id, string doctorId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
            {
                throw new ArgumentException("Identyfikator lekarza jest wymagany.", nameof(doctorId));
            }

            var visit = await _context.Visits.FindAsync(id)
                ?? throw new KeyNotFoundException($"Wizyta o id {id} nie została znaleziona.");

            await EnsureDoctorExistsAsync(doctorId);

            visit.AssignedDoctorId = doctorId;
            await _context.SaveChangesAsync();

            await _context.Entry(visit).Reference(v => v.AssignedDoctor).LoadAsync();
            return _mapper.ToDto(visit);
        }

        public async Task DeleteAsync(int id)
        {
            var visit = await _context.Visits.FindAsync(id)
                ?? throw new KeyNotFoundException($"Wizyta o id {id} nie została znaleziona.");

            if (visit.Status != VisitStatus.Zaplanowana)
            {
                throw new InvalidOperationException(
                    "Można usunąć tylko wizytę ze statusem Zaplanowana.");
            }

            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<VisitListDto>> GetTodayVisitsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var visits = await QueryWithDoctor()
                .Where(v => v.Date >= today && v.Date < tomorrow)
                .OrderBy(v => v.Date)
                .ToListAsync();

            return _mapper.ToListDtos(visits);
        }

        public async Task<IReadOnlyList<ActiveVisitDto>> GetActiveVisitsAsync()
        {
            var visits = await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.AssignedDoctor)
                .Where(v => v.Status != VisitStatus.Anulowana)
                .OrderByDescending(v => v.Date)
                .ToListAsync();

            return visits.Select(ToActiveVisitDto).ToList();
        }

        private IQueryable<Visit> QueryWithDoctor() =>
            _context.Visits
                .Include(v => v.AssignedDoctor)
                .Include(v => v.Patient);

        private IQueryable<Visit> QueryWithDetails() =>
            _context.Visits
                .Include(v => v.AssignedDoctor)
                .Include(v => v.Patient)
                .Include(v => v.ClinicalNotes)
                .Include(v => v.ProceduresPerformed)
                    .ThenInclude(p => p.PrescribedMedications)
                        .ThenInclude(pm => pm.Medication);

        private ActiveVisitDto ToActiveVisitDto(Visit visit)
        {
            var baseDto = _mapper.ToDto(visit);

            return new ActiveVisitDto
            {
                Id = baseDto.Id,
                Date = baseDto.Date,
                Status = baseDto.Status,
                AssignedDoctorId = baseDto.AssignedDoctorId,
                AssignedDoctorName = baseDto.AssignedDoctorName,
                PatientId = baseDto.PatientId,
                Patient = _patientMapper.ToDto(visit.Patient!)
            };
        }

        private VisitDetailDto ToDetailDto(Visit visit)
        {
            var baseDto = _mapper.ToDto(visit);
            var procedures = _procedureMapper.ToDtos(visit.ProceduresPerformed);
            for (var i = 0; i < visit.ProceduresPerformed.Count; i++)
            {
                procedures[i].PrescribedMedications = _prescribedMedicationMapper.ToDtos(
                    visit.ProceduresPerformed[i].PrescribedMedications);
            }

            return new VisitDetailDto
            {
                Id = baseDto.Id,
                Date = baseDto.Date,
                Status = baseDto.Status,
                AssignedDoctorId = baseDto.AssignedDoctorId,
                AssignedDoctorName = baseDto.AssignedDoctorName,
                PatientId = baseDto.PatientId,
                Patient = _patientMapper.ToDto(visit.Patient!),
                Procedures = procedures,
                ClinicalNotes = _clinicalNoteMapper.ToDtos(visit.ClinicalNotes)
            };
        }

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
