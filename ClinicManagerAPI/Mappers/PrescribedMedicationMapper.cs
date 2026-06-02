using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.PrescribedMedications;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class PrescribedMedicationMapper
    {
        [MapProperty(nameof(PrescribedMedication.Medication.Name), nameof(PrescribedMedicationDto.MedicationName))]
        public partial PrescribedMedicationDto ToDto(PrescribedMedication prescribedMedication);

        public partial List<PrescribedMedicationDto> ToDtos(List<PrescribedMedication> prescribedMedications);

        [MapperIgnoreTarget(nameof(PrescribedMedication.Id))]
        [MapperIgnoreTarget(nameof(PrescribedMedication.ProcedurePerformedId))]
        [MapperIgnoreTarget(nameof(PrescribedMedication.Medication))]
        [MapperIgnoreTarget(nameof(PrescribedMedication.ProcedurePerformed))]
        public partial PrescribedMedication ToEntity(CreatePrescribedMedicationDto dto);
    }
}
