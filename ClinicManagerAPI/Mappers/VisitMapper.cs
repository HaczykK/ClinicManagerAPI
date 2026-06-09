using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.Visits;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class VisitMapper
    {
        [MapPropertyFromSource(nameof(VisitDto.AssignedDoctorName), Use = nameof(MapDoctorName))]
        public partial VisitDto ToDto(Visit visit);

        [MapPropertyFromSource(nameof(VisitListDto.AssignedDoctorName), Use = nameof(MapDoctorName))]
        [MapPropertyFromSource(nameof(VisitListDto.PatientName), Use = nameof(MapPatientName))]
        public partial VisitListDto ToListDto(Visit visit);

        public partial List<VisitListDto> ToListDtos(List<Visit> visits);

        [MapperIgnoreTarget(nameof(Visit.Id))]
        [MapperIgnoreTarget(nameof(Visit.Patient))]
        [MapperIgnoreTarget(nameof(Visit.AssignedDoctor))]
        [MapperIgnoreTarget(nameof(Visit.ProceduresPerformed))]
        [MapperIgnoreTarget(nameof(Visit.ClinicalNotes))]
        public partial Visit ToEntity(CreateVisitDto dto);

        [MapperIgnoreTarget(nameof(Visit.Id))]
        [MapperIgnoreTarget(nameof(Visit.Patient))]
        [MapperIgnoreTarget(nameof(Visit.AssignedDoctor))]
        [MapperIgnoreTarget(nameof(Visit.ProceduresPerformed))]
        [MapperIgnoreTarget(nameof(Visit.ClinicalNotes))]
        public partial void ApplyUpdate(UpdateVisitDto dto, Visit visit);

        private static string? MapDoctorName(Visit visit) =>
            visit.AssignedDoctor is null
                ? null
                : $"{visit.AssignedDoctor.FirstName} {visit.AssignedDoctor.LastName}".Trim();

        private static string? MapPatientName(Visit visit) =>
            visit.Patient is null
                ? null
                : $"{visit.Patient.FirstName} {visit.Patient.LastName}".Trim();
    }
}
