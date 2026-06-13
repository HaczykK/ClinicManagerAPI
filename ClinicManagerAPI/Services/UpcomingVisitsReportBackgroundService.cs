using ClinicManagerAPI.Configuration;
using Microsoft.Extensions.Options;

namespace ClinicManagerAPI.Services;

public class UpcomingVisitsReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SmtpSettings _smtpSettings;
    private readonly ReportSettings _reportSettings;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<UpcomingVisitsReportBackgroundService> _logger;

    public UpcomingVisitsReportBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<SmtpSettings> smtpSettings,
        IOptions<ReportSettings> reportSettings,
        IHostEnvironment environment,
        ILogger<UpcomingVisitsReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _smtpSettings = smtpSettings.Value;
        _reportSettings = reportSettings.Value;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpcomingVisitsReportBackgroundService uruchomiony");

        while (!stoppingToken.IsCancellationRequested)
        {
            var tomorrow = DateTime.UtcNow.Date.AddDays(1);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();

                _logger.LogInformation("Generowanie raportu wizyt na {Date:dd.MM.yyyy}", tomorrow);

                var pdf = await pdfService.GenerateUpcomingVisitsPdf(tomorrow);
                var reportPath = await SaveReportAsync(pdf, stoppingToken);

                _logger.LogInformation(
                    "Raport wizyt na {Date:dd.MM.yyyy} zapisany jako {ReportPath}",
                    tomorrow,
                    reportPath);

                if (IsSmtpConfigured())
                {
                    try
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var attachmentName = $"upcoming_visits_{tomorrow:dd.MM.yyyy}.pdf";

                        await emailService.SendEmailWithAttachmentAsync(
                            _smtpSettings.AdminEmail,
                            $"Raport wizyt na {tomorrow:dd.MM.yyyy}",
                            $"W załączniku znajduje się raport PDF z wizytami zaplanowanymi na {tomorrow:dd.MM.yyyy}.",
                            pdf,
                            attachmentName);

                        _logger.LogInformation(
                            "Raport wizyt na {Date:dd.MM.yyyy} wysłany na {AdminEmail}",
                            tomorrow,
                            _smtpSettings.AdminEmail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Raport zapisany, ale wysyłka e-mail nie powiodła się");
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Pominięto wysyłkę e-mail — uzupełnij SmtpSettings:Username i SmtpSettings:Password w appsettings.json (np. dane z Mailtrap)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas generowania raportu wizyt");
            }

            await Task.Delay(TimeSpan.FromMinutes(_reportSettings.IntervalMinutes), stoppingToken);
        }
    }

    private async Task<string> SaveReportAsync(byte[] pdf, CancellationToken cancellationToken)
    {
        var outputDir = Path.Combine(_environment.ContentRootPath, _reportSettings.OutputDirectory);
        Directory.CreateDirectory(outputDir);

        var reportPath = Path.Combine(outputDir, _reportSettings.FileName);
        await File.WriteAllBytesAsync(reportPath, pdf, cancellationToken);

        return reportPath;
    }

    private bool IsSmtpConfigured() =>
        !string.IsNullOrWhiteSpace(_smtpSettings.Host)
        && !string.IsNullOrWhiteSpace(_smtpSettings.Username)
        && !string.IsNullOrWhiteSpace(_smtpSettings.Password)
        && !_smtpSettings.Username.StartsWith("your-", StringComparison.OrdinalIgnoreCase);
}
