using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClinicManagerAPI.Models;

namespace ClinicManagerAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<ProcedurePerformed> ProceduresPerformed { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<PrescribedMedication> PrescribedMedications { get; set; }
        public DbSet<ClinicalNote> ClinicalNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProcedurePerformed>()
                .Property(p => p.ServiceCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Medication>()
                .Property(m => m.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
