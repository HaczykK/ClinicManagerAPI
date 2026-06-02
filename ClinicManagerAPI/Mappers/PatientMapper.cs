using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.Patients;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class PatientMapper
    {
        public partial PatientDto ToDto(Patient patient);

        public partial PatientListDto ToListDto(Patient patient);

        public partial List<PatientListDto> ToListDtos(List<Patient> patients);

        [MapperIgnoreTarget(nameof(Patient.Id))]
        [MapperIgnoreTarget(nameof(Patient.Visits))]
        [MapperIgnoreTarget(nameof(Patient.MedicalRecords))]
        [MapperIgnoreTarget(nameof(Patient.IsDeleted))]
        [MapperIgnoreTarget(nameof(Patient.DeletedAt))]
        public partial Patient ToEntity(CreatePatientDto dto);

        [MapperIgnoreTarget(nameof(Patient.Id))]
        [MapperIgnoreTarget(nameof(Patient.Visits))]
        [MapperIgnoreTarget(nameof(Patient.MedicalRecords))]
        [MapperIgnoreTarget(nameof(Patient.IsDeleted))]
        [MapperIgnoreTarget(nameof(Patient.DeletedAt))]
        public partial void ApplyUpdate(UpdatePatientDto dto, Patient patient);
    }
}
