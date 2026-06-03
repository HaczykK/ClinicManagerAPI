using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.MedicalRecords;
using ClinicManagerAPI.Mappers;
using ClinicManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".pdf"
        };

        private readonly ApplicationDbContext _context;
        private readonly MedicalRecordMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public MedicalRecordService(
            ApplicationDbContext context,
            MedicalRecordMapper mapper,
            IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
        {
            await EnsurePatientExistsAsync(patientId);

            var records = await _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            return _mapper.ToDtos(records);
        }

        public async Task<MedicalRecordDto?> GetByIdAsync(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            return record is null ? null : _mapper.ToDto(record);
        }

        public async Task<MedicalRecordDto> CreateAsync(int patientId, IFormFile file, string? description)
        {
            await EnsurePatientExistsAsync(patientId);
            ValidateFile(file);

            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var uploadsDir = GetUploadsDirectory();
            Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, storedFileName);
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var record = new MedicalRecord
            {
                PatientId = patientId,
                DocumentScanUrl = $"uploads/{storedFileName}",
                FileName = file.FileName,
                Description = description,
                UploadedAt = DateTime.UtcNow
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return _mapper.ToDto(record);
        }

        public async Task DeleteAsync(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id)
                ?? throw new KeyNotFoundException($"Dokument kartoteki o id {id} nie został znaleziony.");

            TryDeletePhysicalFile(record.DocumentScanUrl);

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
        }

        private static void ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                throw new ArgumentException("Plik jest pusty.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("Plik przekracza maksymalny rozmiar 5 MB.");

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
                throw new ArgumentException("Niedozwolony typ pliku. Dozwolone: jpg, png, pdf.");
        }

        private string GetUploadsDirectory()
        {
            var webRoot = _environment.WebRootPath
                ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            return Path.Combine(webRoot, "uploads");
        }

        private void TryDeletePhysicalFile(string? documentScanUrl)
        {
            if (string.IsNullOrWhiteSpace(documentScanUrl))
                return;

            var uploadsDir = GetUploadsDirectory();
            var fullPath = Path.GetFullPath(Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                documentScanUrl.Replace('/', Path.DirectorySeparatorChar)));

            var uploadsDirFull = Path.GetFullPath(uploadsDir);
            if (!fullPath.StartsWith(uploadsDirFull, StringComparison.OrdinalIgnoreCase))
                return;

            if (File.Exists(fullPath))
                File.Delete(fullPath);
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
