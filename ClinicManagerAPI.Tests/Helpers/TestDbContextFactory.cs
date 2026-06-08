using ClinicManagerAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagerAPI.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
