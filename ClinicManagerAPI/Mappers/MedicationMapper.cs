using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.Medications;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class MedicationMapper
    {
        public partial MedicationDto ToDto(Medication medication);

        public partial List<MedicationDto> ToDtos(List<Medication> medications);

        [MapperIgnoreTarget(nameof(Medication.Id))]
        public partial Medication ToEntity(CreateMedicationDto dto);

        [MapperIgnoreTarget(nameof(Medication.Id))]
        public partial void ApplyUpdate(UpdateMedicationDto dto, Medication medication);
    }
}
