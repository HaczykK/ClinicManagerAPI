using Riok.Mapperly.Abstractions;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.DTOs.ClinicalNotes;

namespace ClinicManagerAPI.Mappers
{
    [Mapper]
    public partial class ClinicalNoteMapper
    {
        public partial ClinicalNoteDto ToDto(ClinicalNote clinicalNote);

        public partial List<ClinicalNoteDto> ToDtos(List<ClinicalNote> clinicalNotes);

        [MapperIgnoreTarget(nameof(ClinicalNote.Id))]
        [MapperIgnoreTarget(nameof(ClinicalNote.Timestamp))]
        [MapperIgnoreTarget(nameof(ClinicalNote.Visit))]
        public partial ClinicalNote ToEntity(CreateClinicalNoteDto dto);

        [MapperIgnoreTarget(nameof(ClinicalNote.Id))]
        [MapperIgnoreTarget(nameof(ClinicalNote.Timestamp))]
        [MapperIgnoreTarget(nameof(ClinicalNote.VisitId))]
        [MapperIgnoreTarget(nameof(ClinicalNote.Visit))]
        public partial void ApplyUpdate(UpdateClinicalNoteDto dto, ClinicalNote clinicalNote);
    }
}
