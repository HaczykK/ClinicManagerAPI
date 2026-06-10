using ClinicManagerAPI.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicManagerAPI.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailWithAttachmentAsync(
        string to,
        string subject,
        string body,
        byte[] attachment,
        string attachmentName)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = body
        };

        builder.Attachments.Add(attachmentName, attachment, ContentType.Parse("application/pdf"));
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            await client.AuthenticateAsync(_settings.Username, _settings.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("E-mail wysłany do {Recipient} z załącznikiem {AttachmentName}", to, attachmentName);
    }
}
