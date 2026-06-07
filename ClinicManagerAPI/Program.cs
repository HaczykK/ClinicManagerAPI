using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NLog;
using NLog.Web;
using ClinicManagerAPI.Data;
using ClinicManagerAPI.Mappers;
using ClinicManagerAPI.Middleware;
using ClinicManagerAPI.Models;
using ClinicManagerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguracja ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Konfiguracja JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "TymczasowyKluczDoZmianyWProdukcji123!@#";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ClinicManagerAPI",
        ValidAudience = jwtSettings["Audience"] ?? "ClinicManagerClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<PatientMapper>();
builder.Services.AddScoped<VisitMapper>();
builder.Services.AddScoped<MedicationMapper>();
builder.Services.AddScoped<MedicalRecordMapper>();
builder.Services.AddScoped<ClinicalNoteMapper>();
builder.Services.AddScoped<ProcedureMapper>();
builder.Services.AddScoped<PrescribedMedicationMapper>();

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
builder.Services.AddScoped<IClinicalNoteService, ClinicalNoteService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed ról i danych testowych przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "Lekarz", "Rejestratorka" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

// Globalny middleware do obsługi wyjątków (musi być pierwszy w pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
