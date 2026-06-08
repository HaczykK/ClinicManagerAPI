using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Visits;
using ClinicManagerAPI.Mappers;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;
using ClinicManagerAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Tests.Services;

public class VisitServiceTests
{
    private static VisitService CreateService(ApplicationDbContext context) =>
        new(
            context,
            new VisitMapper(),
            new PatientMapper(),
            new ClinicalNoteMapper(),
            new ProcedureMapper(),
            new PrescribedMedicationMapper());

    private static async Task<Patient> SeedPatientAsync(ApplicationDbContext context)
    {
        var patient = new Patient
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Pesel = "90010112345"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    private static async Task<ApplicationUser> SeedDoctorAsync(ApplicationDbContext context)
    {
        var doctor = new ApplicationUser
        {
            Id = "doctor-1",
            UserName = "doctor@test.com",
            Email = "doctor@test.com",
            FirstName = "Andrzej",
            LastName = "Lekarski"
        };
        context.Users.Add(doctor);
        await context.SaveChangesAsync();
        return doctor;
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedVisit()
    {
        using var context = TestDbContextFactory.CreateContext();
        var patient = await SeedPatientAsync(context);
        var doctor = await SeedDoctorAsync(context);
        var service = CreateService(context);
        var dto = new CreateVisitDto
        {
            Date = DateTime.Today.AddHours(10),
            Status = VisitStatus.Zaplanowana,
            PatientId = patient.Id,
            AssignedDoctorId = doctor.Id
        };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal(VisitStatus.Zaplanowana, result.Status);
        Assert.Equal(doctor.Id, result.AssignedDoctorId);

        var saved = await context.Visits.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(patient.Id, saved.PatientId);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_UpdatesStatus()
    {
        using var context = TestDbContextFactory.CreateContext();
        var patient = await SeedPatientAsync(context);
        var doctor = await SeedDoctorAsync(context);
        var visit = new Visit
        {
            Date = DateTime.Today.AddHours(10),
            Status = VisitStatus.Zaplanowana,
            PatientId = patient.Id,
            AssignedDoctorId = doctor.Id
        };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.UpdateStatusAsync(visit.Id, VisitStatus.WTrakcie);

        Assert.Equal(VisitStatus.WTrakcie, result.Status);

        var updated = await context.Visits.FindAsync(visit.Id);
        Assert.NotNull(updated);
        Assert.Equal(VisitStatus.WTrakcie, updated.Status);
    }

    [Fact]
    public async Task GetTodayVisitsAsync_ReturnsOnlyTodayVisits()
    {
        using var context = TestDbContextFactory.CreateContext();
        var patient = await SeedPatientAsync(context);
        var doctor = await SeedDoctorAsync(context);

        context.Visits.AddRange(
            new Visit
            {
                Date = DateTime.Today.AddDays(-1).AddHours(10),
                Status = VisitStatus.Zaplanowana,
                PatientId = patient.Id,
                AssignedDoctorId = doctor.Id
            },
            new Visit
            {
                Date = DateTime.Today.AddHours(14),
                Status = VisitStatus.Zaplanowana,
                PatientId = patient.Id,
                AssignedDoctorId = doctor.Id
            },
            new Visit
            {
                Date = DateTime.Today.AddDays(1).AddHours(10),
                Status = VisitStatus.Zaplanowana,
                PatientId = patient.Id,
                AssignedDoctorId = doctor.Id
            });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetTodayVisitsAsync();

        Assert.Single(result);
        Assert.Equal(DateTime.Today, result[0].Date.Date);
    }
}
