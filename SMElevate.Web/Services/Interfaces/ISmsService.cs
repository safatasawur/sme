namespace SMElevate.Web.Services.Interfaces;

// PENDING: No SMS provider is configured. Implement with Twilio/Telenor/Jazz API when available.
public interface ISmsService
{
    Task<bool> SendAsync(string mobileNumber, string message);
}
