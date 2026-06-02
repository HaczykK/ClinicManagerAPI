using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.Visits;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class VisitMapper
    {
        public partial VisitDto ToDto(Visit visit);

        public partial VisitListDto ToListDto(Visit visit);

        public partial List<VisitListDto> ToListDtos(List<Visit> visits);

        [MapperIgnoreTarget(nameof(Visit.Id))]
        [MapperIgnoreTarget(nameof(Visit.Patient))]
        [MapperIgnoreTarget(nameof(Visit.ProceduresPerformed))]
        [MapperIgnoreTarget(nameof(Visit.ClinicalNotes))]
        public partial Visit ToEntity(CreateVisitDto dto);

        [MapperIgnoreTarget(nameof(Visit.Id))]
        [MapperIgnoreTarget(nameof(Visit.Patient))]
        [MapperIgnoreTarget(nameof(Visit.ProceduresPerformed))]
        [MapperIgnoreTarget(nameof(Visit.ClinicalNotes))]
        public partial void ApplyUpdate(UpdateVisitDto dto, Visit visit);
    }
}
