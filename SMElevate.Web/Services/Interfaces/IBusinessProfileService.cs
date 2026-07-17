using SMElevate.Web.Models.Common;

namespace SMElevate.Web.Services.Interfaces;

public interface IBusinessProfileService
{
    Task<List<BusinessProfile>> GetByUserAsync(int userId, bool activeOnly = true);
    Task<BusinessProfile?> GetByIdAsync(int id);
    Task<BusinessProfile?> GetByIdForUserAsync(int id, int userId);
    Task<bool> UserHasVerifiedBusinessAsync(int userId);
    Task<bool> UserHasAnyActiveBusinessAsync(int userId);
    Task<BusinessProfile?> GetFirstPendingVerificationAsync(int userId);

    Task<BusinessProfile> CreateAsync(BusinessProfile profile, List<BusinessShareholder> shareholders);
    Task<BusinessProfile> UpdateAsync(BusinessProfile profile, List<BusinessShareholder> shareholders,
        bool mobileChanged, bool emailChanged, IConfiguration config, IEmailService email, ISmsService sms);
    Task<(bool Success, string Message)> ArchiveAsync(int id, int userId);
    Task<bool> IsLinkedToLoanRequestAsync(int id);

    // OTP operations
    Task<(bool Success, string? Error)> SendEmailOtpAsync(int id, int userId, IConfiguration config, IEmailService email);
    Task<(bool Success, string? Error)> SendMobileOtpAsync(int id, int userId, IConfiguration config, ISmsService sms);
    Task<(bool Success, string? Error)> VerifyEmailOtpAsync(int id, int userId, string otp, IConfiguration config);
    Task<(bool Success, string? Error)> VerifyMobileOtpAsync(int id, int userId, string otp, IConfiguration config);
    Task<(bool Success, string? Error)> ResendEmailOtpAsync(int id, int userId, IConfiguration config, IEmailService email);
    Task<(bool Success, string? Error)> ResendMobileOtpAsync(int id, int userId, IConfiguration config, ISmsService sms);
}
