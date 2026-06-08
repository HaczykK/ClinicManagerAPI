using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Patients;
using ClinicManagerAPI.Mappers;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;
using ClinicManagerAPI.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Tests.Services;

public class PatientServiceTests
{
    private static PatientService CreateService(ApplicationDbContext context) =>
        new(context, new PatientMapper());

    [Fact]
    public async Task GetAllAsync_ReturnsAllPatients()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Patients.AddRange(
            new Patient { FirstName = "Jan", LastName = "Kowalski", Pesel = "90010112345" },
            new Patient { FirstName = "Anna", LastName = "Nowak", Pesel = "85050567890" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetAllAsync(page: 1, pageSize: 10);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, p => p.Pesel == "90010112345");
        Assert.Contains(result.Items, p => p.Pesel == "85050567890");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsPatient()
    {
        using var context = TestDbContextFactory.CreateContext();
        var patient = new Patient { FirstName = "Jan", LastName = "Kowalski", Pesel = "90010112345" };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetByIdAsync(patient.Id);

        Assert.NotNull(result);
        Assert.Equal(patient.Id, result.Id);
        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("90010112345", result.Pesel);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedPatient()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateService(context);
        var dto = new CreatePatientDto
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Pesel = "90010112345",
            InsuranceNumber = "INS123"
        };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("Kowalski", result.LastName);
        Assert.Equal("90010112345", result.Pesel);
        Assert.Equal("INS123", result.InsuranceNumber);

        var saved = await context.Patients.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal("90010112345", saved.Pesel);
    }

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedTrue()
    {
        using var context = TestDbContextFactory.CreateContext();
        var patient = new Patient { FirstName = "Jan", LastName = "Kowalski", Pesel = "90010112345" };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        await service.DeleteAsync(patient.Id);

        var deleted = await context.Patients
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == patient.Id);

        Assert.True(deleted.IsDeleted);
        Assert.NotNull(deleted.DeletedAt);
    }

    [Fact]
    public async Task SearchAsync_ByPesel_ReturnsMatchingPatients()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Patients.AddRange(
            new Patient { FirstName = "Jan", LastName = "Kowalski", Pesel = "90010112345" },
            new Patient { FirstName = "Anna", LastName = "Nowak", Pesel = "85050567890" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SearchAsync("900101");

        Assert.Single(result);
        Assert.Equal("90010112345", result[0].Pesel);
    }

    [Fact]
    public async Task SearchAsync_ByLastName_ReturnsMatchingPatients()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Patients.AddRange(
            new Patient { FirstName = "Jan", LastName = "Kowalski", Pesel = "90010112345" },
            new Patient { FirstName = "Anna", LastName = "Nowak", Pesel = "85050567890" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.SearchAsync("Kowalski");

        Assert.Single(result);
        Assert.Equal("Kowalski", result[0].LastName);
    }
}
