using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.ClinicalNotes;
using ClinicManagerAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class ClinicalNoteService : IClinicalNoteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ClinicalNoteMapper _mapper;

        public ClinicalNoteService(ApplicationDbContext context, ClinicalNoteMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ClinicalNoteDto>> GetByVisitIdAsync(int visitId)
        {
            await EnsureVisitExistsAsync(visitId);

            var notes = await _context.ClinicalNotes
                .Where(n => n.VisitId == visitId)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            return _mapper.ToDtos(notes);
        }

        public async Task<ClinicalNoteDto?> GetByIdAsync(int id)
        {
            var note = await _context.ClinicalNotes.FindAsync(id);
            return note is null ? null : _mapper.ToDto(note);
        }

        public async Task<ClinicalNoteDto> CreateAsync(CreateClinicalNoteDto dto)
        {
            await EnsureVisitExistsAsync(dto.VisitId);

            var note = _mapper.ToEntity(dto);
            note.Timestamp = DateTime.UtcNow;
            _context.ClinicalNotes.Add(note);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(note);
        }

        public async Task<ClinicalNoteDto> UpdateAsync(int id, UpdateClinicalNoteDto dto)
        {
            var note = await _context.ClinicalNotes.FindAsync(id)
                ?? throw new KeyNotFoundException($"Notatka kliniczna o id {id} nie została znaleziona.");

            _mapper.ApplyUpdate(dto, note);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(note);
        }

        public async Task DeleteAsync(int id)
        {
            var note = await _context.ClinicalNotes.FindAsync(id)
                ?? throw new KeyNotFoundException($"Notatka kliniczna o id {id} nie została znaleziona.");

            _context.ClinicalNotes.Remove(note);
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
