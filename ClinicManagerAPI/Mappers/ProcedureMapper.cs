using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.Procedures;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class ProcedureMapper
    {
        [MapperIgnoreTarget(nameof(ProcedurePerformedDto.PrescribedMedications))]
        public partial ProcedurePerformedDto ToDto(ProcedurePerformed procedure);

        public partial List<ProcedurePerformedDto> ToDtos(List<ProcedurePerformed> procedures);

        [MapperIgnoreTarget(nameof(ProcedurePerformed.Id))]
        [MapperIgnoreTarget(nameof(ProcedurePerformed.Visit))]
        [MapperIgnoreTarget(nameof(ProcedurePerformed.PrescribedMedications))]
        public partial ProcedurePerformed ToEntity(CreateProcedurePerformedDto dto);
    }
}
