using ClinicManagerAPI.Data;
using ClinicManagerAPI.DTOs.Medications;
using ClinicManagerAPI.Mappers;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;
using ClinicManagerAPI.Tests.Helpers;

namespace ClinicManagerAPI.Tests.Services;

public class MedicationServiceTests
{
    private static MedicationService CreateService(ApplicationDbContext context) =>
        new(context, new MedicationMapper());

    [Fact]
    public async Task GetAllAsync_ReturnsAllMedications()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Medications.AddRange(
            new Medication { Name = "Ibuprofen", UnitPrice = 12.50m },
            new Medication { Name = "Paracetamol", UnitPrice = 8.00m });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Name == "Ibuprofen");
        Assert.Contains(result, m => m.Name == "Paracetamol");
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedMedication()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = CreateService(context);
        var dto = new CreateMedicationDto
        {
            Name = "Aspirin",
            UnitPrice = 15.99m
        };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Aspirin", result.Name);
        Assert.Equal(15.99m, result.UnitPrice);

        var saved = await context.Medications.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal("Aspirin", saved.Name);
        Assert.Equal(15.99m, saved.UnitPrice);
    }
}
