using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Medications;
using ClinicManagerAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly MedicationMapper _mapper;

        public MedicationService(ApplicationDbContext context, MedicationMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<MedicationDto>> GetAllAsync(string? name = null)
        {
            var query = _context.Medications.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalized = name.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(normalized));
            }

            var medications = await query
                .OrderBy(m => m.Name)
                .ToListAsync();

            return _mapper.ToDtos(medications);
        }

        public async Task<MedicationDto?> GetByIdAsync(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            return medication is null ? null : _mapper.ToDto(medication);
        }

        public async Task<MedicationDto> CreateAsync(CreateMedicationDto dto)
        {
            var medication = _mapper.ToEntity(dto);
            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(medication);
        }

        public async Task<MedicationDto> UpdateAsync(int id, UpdateMedicationDto dto)
        {
            var medication = await _context.Medications.FindAsync(id)
                ?? throw new KeyNotFoundException($"Lek o id {id} nie został znaleziony.");

            _mapper.ApplyUpdate(dto, medication);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(medication);
        }

        public async Task DeleteAsync(int id)
        {
            var medication = await _context.Medications.FindAsync(id)
                ?? throw new KeyNotFoundException($"Lek o id {id} nie został znaleziony.");

            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();
        }
    }
}
