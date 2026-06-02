using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.MedicalRecords;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class MedicalRecordMapper
    {
        public partial MedicalRecordDto ToDto(MedicalRecord medicalRecord);

        public partial List<MedicalRecordDto> ToDtos(List<MedicalRecord> medicalRecords);

        [MapperIgnoreTarget(nameof(MedicalRecord.Id))]
        [MapperIgnoreTarget(nameof(MedicalRecord.Patient))]
        public partial MedicalRecord ToEntity(CreateMedicalRecordDto dto);
    }
}
