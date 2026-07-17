namespace SMElevate.Web.Services.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, string? templateCode = null, string? ccEmail = null);
    Task SendFromTemplateAsync(string templateCode, string toEmail, Dictionary<string, string> placeholders, string? ccEmail = null);
}
