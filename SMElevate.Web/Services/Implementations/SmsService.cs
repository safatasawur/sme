using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

// Stub implementation — logs instead of sending until an SMS gateway is configured.
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger) => _logger = logger;

    public Task<bool> SendAsync(string mobileNumber, string message)
    {
        // Mask number in log: 0300******7
        var masked = mobileNumber.Length > 4
            ? mobileNumber[..4] + new string('*', Math.Max(0, mobileNumber.Length - 5)) + mobileNumber[^1]
            : "****";
        _logger.LogInformation("[SMS-PENDING] To {Masked}: {Length}-char message (SMS provider not configured)",
            masked, message.Length);
        return Task.FromResult(true);
    }
}
