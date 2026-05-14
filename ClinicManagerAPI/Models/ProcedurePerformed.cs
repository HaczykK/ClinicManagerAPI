using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagerAPI.Models
{
    public class ProcedurePerformed
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceCost { get; set; }

        // Relationship with Visit
        public int VisitId { get; set; }
        public Visit? Visit { get; set; }

        // Relationship with Prescribed Medications
        public List<PrescribedMedication> PrescribedMedications { get; set; } = new List<PrescribedMedication>();
    }
}
