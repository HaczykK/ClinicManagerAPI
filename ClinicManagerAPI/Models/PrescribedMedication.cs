using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.Models
{
    public class PrescribedMedication
    {
        public int Id { get; set; }

        public int MedicationId { get; set; }
        public Medication? Medication { get; set; }

        public int ProcedurePerformedId { get; set; }
        public ProcedurePerformed? ProcedurePerformed { get; set; }

        [Required]
        public string Dosage { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od zera.")]
        public int Quantity { get; set; }
    }
}
