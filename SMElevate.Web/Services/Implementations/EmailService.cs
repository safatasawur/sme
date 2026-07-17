using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailTemplateService _templates;
    private readonly ISettingsService _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(ApplicationDbContext db, IEmailTemplateService templates,
        ISettingsService settings, ILogger<EmailService> logger)
    {
        _db = db;
        _templates = templates;
        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody,
        string? templateCode = null, string? ccEmail = null)
    {
        var log = new EmailLog
        {
            ToEmail = toEmail,
            CcEmail = ccEmail,
            Subject = subject,
            BodyHtml = htmlBody,
            TemplateCode = templateCode,
            Status = EmailStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _db.EmailLogs.Add(log);
        await _db.SaveChangesAsync();

        // Load all SMTP settings from DB in one query
        var smtp = await _settings.GetCategoryAsync(SettingCategory.Email);

        var enabled = string.Equals(smtp.GetValueOrDefault("Smtp.Enabled"), "true", StringComparison.OrdinalIgnoreCase);
        if (!enabled)
        {
            log.Status = EmailStatus.Failed;
            log.ErrorMessage = "SMTP is disabled. Enable it in Admin → Settings → Email Settings.";
            await _db.SaveChangesAsync();
            _logger.LogInformation("Email skipped (SMTP disabled): {Subject} -> {To}", subject, toEmail);
            return;
        }

        var host     = smtp.GetValueOrDefault("Smtp.Host") ?? "";
        var port     = int.TryParse(smtp.GetValueOrDefault("Smtp.Port"), out var p) ? p : 587;
        var username = smtp.GetValueOrDefault("Smtp.Username") ?? "";
        var password = smtp.GetValueOrDefault("Smtp.Password") ?? "";
        var fromEmail = smtp.GetValueOrDefault("Smtp.FromEmail") ?? "";
        var fromName  = smtp.GetValueOrDefault("Smtp.FromName") ?? "SMElevate Portal";
        var enableSsl = !string.Equals(smtp.GetValueOrDefault("Smtp.EnableSsl"), "false", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(host))
        {
            log.Status = EmailStatus.Failed;
            log.ErrorMessage = "SMTP Host is not configured. Set it in Admin → Settings → Email Settings.";
            await _db.SaveChangesAsync();
            _logger.LogWarning("Email skipped (no SMTP host): {Subject} -> {To}", subject, toEmail);
            return;
        }

        try
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(fromName, fromEmail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            if (!string.IsNullOrEmpty(ccEmail)) msg.Cc.Add(MailboxAddress.Parse(ccEmail));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port,
                enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            if (!string.IsNullOrEmpty(username))
                await client.AuthenticateAsync(username, password);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            log.Status = EmailStatus.Sent;
            log.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            log.Status = EmailStatus.Failed;
            log.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Email send failed: {Subject} -> {To}", subject, toEmail);
        }
        finally
        {
            await _db.SaveChangesAsync();
        }
    }

    public async Task SendFromTemplateAsync(string templateCode, string toEmail,
        Dictionary<string, string> placeholders, string? ccEmail = null)
    {
        var template = await _templates.GetByCodeAsync(templateCode);
        if (template is null || !template.IsActive)
        {
            _logger.LogWarning("Email template not found or inactive: {Code}", templateCode);
            return;
        }
        var subject = _templates.ReplacePlaceholders(template.Subject, placeholders);
        var body = _templates.ReplacePlaceholders(template.BodyHtml, placeholders);
        await SendAsync(toEmail, subject, body, templateCode, ccEmail);
    }
}
