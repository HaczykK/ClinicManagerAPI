using ClinicManagerAPI.Configuration;
using Microsoft.Extensions.Options;

namespace ClinicManagerAPI.Services;

public class UpcomingVisitsReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<UpcomingVisitsReportBackgroundService> _logger;

    public UpcomingVisitsReportBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<SmtpSettings> smtpSettings,
        ILogger<UpcomingVisitsReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UpcomingVisitsReportBackgroundService uruchomiony");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var tomorrow = DateTime.Today.AddDays(1);
                _logger.LogInformation("Generowanie raportu wizyt na {Date:dd.MM.yyyy}", tomorrow);

                var pdf = await pdfService.GenerateUpcomingVisitsPdf(tomorrow);
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
                _logger.LogError(ex, "Błąd podczas generowania lub wysyłki raportu wizyt");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
