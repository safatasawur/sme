using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class BusinessProfileService : IBusinessProfileService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BusinessProfileService> _logger;

    public BusinessProfileService(ApplicationDbContext db, ILogger<BusinessProfileService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<List<BusinessProfile>> GetByUserAsync(int userId, bool activeOnly = true)
    {
        var q = _db.BusinessProfiles
            .Include(b => b.Shareholders)
            .Where(b => b.UserId == userId);
        if (activeOnly) q = q.Where(b => b.IsActive);
        return await q.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<BusinessProfile?> GetByIdAsync(int id) =>
        await _db.BusinessProfiles
            .Include(b => b.Shareholders)
            .Include(b => b.BusinessBank)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<BusinessProfile?> GetByIdForUserAsync(int id, int userId) =>
        await _db.BusinessProfiles
            .Include(b => b.Shareholders)
            .Include(b => b.BusinessBank)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

    public async Task<bool> UserHasVerifiedBusinessAsync(int userId) =>
        await _db.BusinessProfiles.AnyAsync(b =>
            b.UserId == userId && b.IsActive &&
            b.BusinessVerificationStatus == Models.Common.BusinessVerificationStatus.Verified);

    public async Task<bool> UserHasAnyActiveBusinessAsync(int userId) =>
        await _db.BusinessProfiles.AnyAsync(b => b.UserId == userId && b.IsActive);

    public async Task<BusinessProfile?> GetFirstPendingVerificationAsync(int userId) =>
        await _db.BusinessProfiles.FirstOrDefaultAsync(b =>
            b.UserId == userId && b.IsActive &&
            b.BusinessVerificationStatus != Models.Common.BusinessVerificationStatus.Verified);

    public async Task<bool> IsLinkedToLoanRequestAsync(int id) =>
        await _db.LoanRequests.AnyAsync(r => r.BusinessProfileId == id);

    // ── CRUD ──────────────────────────────────────────────────────────────────

    public async Task<BusinessProfile> CreateAsync(BusinessProfile profile, List<BusinessShareholder> shareholders)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.IsActive = true;
        profile.BusinessVerificationStatus = Models.Common.BusinessVerificationStatus.Pending;
        _db.BusinessProfiles.Add(profile);
        await _db.SaveChangesAsync();
        foreach (var sh in shareholders.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
        {
            sh.BusinessProfileId = profile.Id;
            sh.CreatedAt = DateTime.UtcNow;
            _db.BusinessShareholders.Add(sh);
        }
        await _db.SaveChangesAsync();
        _logger.LogInformation("Business profile {Id} created for user {UserId}", profile.Id, profile.UserId);
        return profile;
    }

    public async Task<BusinessProfile> UpdateAsync(BusinessProfile profile, List<BusinessShareholder> shareholders,
        bool mobileChanged, bool emailChanged, IConfiguration config, IEmailService email, ISmsService sms)
    {
        profile.UpdatedAt = DateTime.UtcNow;

        if (mobileChanged)
        {
            profile.IsBusinessMobileVerified = false;
            profile.BusinessMobileOtpCode = null;
            profile.BusinessMobileOtpExpiry = null;
            profile.MobileOtpAttempts = 0;
            profile.MobileResendCount = 0;
        }
        if (emailChanged)
        {
            profile.IsBusinessEmailVerified = false;
            profile.BusinessEmailOtpCode = null;
            profile.BusinessEmailOtpExpiry = null;
            profile.EmailOtpAttempts = 0;
            profile.EmailResendCount = 0;
        }
        RecalcVerificationStatus(profile);

        _db.BusinessProfiles.Update(profile);

        // Replace shareholders
        var existing = await _db.BusinessShareholders
            .Where(s => s.BusinessProfileId == profile.Id).ToListAsync();
        _db.BusinessShareholders.RemoveRange(existing);
        foreach (var sh in shareholders.Where(s => !string.IsNullOrWhiteSpace(s.Name)))
        {
            sh.BusinessProfileId = profile.Id;
            sh.CreatedAt = DateTime.UtcNow;
            _db.BusinessShareholders.Add(sh);
        }
        await _db.SaveChangesAsync();

        if (mobileChanged)
            await SendMobileOtpAsync(profile.Id, profile.UserId, config, sms);
        if (emailChanged)
            await SendEmailOtpAsync(profile.Id, profile.UserId, config, email);

        _logger.LogInformation("Business profile {Id} updated for user {UserId}", profile.Id, profile.UserId);
        return profile;
    }

    public async Task<(bool Success, string Message)> ArchiveAsync(int id, int userId)
    {
        var profile = await _db.BusinessProfiles
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (profile is null) return (false, "Business not found.");

        if (await IsLinkedToLoanRequestAsync(id))
            return (false, "This business cannot be deleted because it is associated with one or more loan requests. You may edit the business or mark it inactive.");

        profile.IsActive = false;
        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Business profile {Id} archived by user {UserId}", id, userId);
        return (true, "Business has been archived successfully.");
    }

    // ── OTP: Email ────────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> SendEmailOtpAsync(
        int id, int userId, IConfiguration config, IEmailService email)
    {
        var bp = await _db.BusinessProfiles.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);
        if (bp is null) return (false, "Business not found.");
        if (bp.IsBusinessEmailVerified) return (false, "Email is already verified.");

        var cfg = OtpConfig(config);
        var maxResend = cfg.MaxResend;

        if (bp.EmailResendCount >= maxResend)
            return (false, $"Maximum OTP resend limit ({maxResend}) reached.");

        var otp = GenerateOtp(cfg.Length);
        bp.BusinessEmailOtpCode = otp;
        bp.BusinessEmailOtpExpiry = DateTime.UtcNow.AddMinutes(cfg.ExpiryMinutes);
        bp.EmailOtpAttempts = 0;
        bp.EmailResendCount++;
        bp.LastEmailOtpSentAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var masked = MaskEmail(bp.BusinessEmailAddress);
        try
        {
            await email.SendFromTemplateAsync("BP_EMAIL_OTP", bp.BusinessEmailAddress, new()
            {
                ["BusinessName"] = bp.NameOfBusiness,
                ["OtpCode"] = otp,
                ["ExpiryMinutes"] = cfg.ExpiryMinutes.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email OTP for business {Id}", id);
        }
        _logger.LogInformation("Email OTP sent for business {Id} to {Masked}", id, masked);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> VerifyEmailOtpAsync(
        int id, int userId, string otp, IConfiguration config)
    {
        var bp = await _db.BusinessProfiles.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);
        if (bp is null) return (false, "Business not found.");
        if (bp.IsBusinessEmailVerified) return (true, null);

        var cfg = OtpConfig(config);
        if (bp.EmailOtpAttempts >= cfg.MaxAttempts)
            return (false, "Too many failed attempts. Please request a new OTP.");
        if (string.IsNullOrEmpty(bp.BusinessEmailOtpCode) || bp.BusinessEmailOtpExpiry < DateTime.UtcNow)
            return (false, "OTP has expired. Please request a new one.");
        if (bp.BusinessEmailOtpCode != otp.Trim())
        {
            bp.EmailOtpAttempts++;
            await _db.SaveChangesAsync();
            return (false, "Invalid OTP. Please try again.");
        }
        bp.IsBusinessEmailVerified = true;
        bp.BusinessEmailOtpCode = null;
        bp.BusinessEmailOtpExpiry = null;
        bp.EmailOtpAttempts = 0;
        RecalcVerificationStatus(bp);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Email verified for business {Id}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ResendEmailOtpAsync(
        int id, int userId, IConfiguration config, IEmailService email)
    {
        var bp = await _db.BusinessProfiles.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);
        if (bp is null) return (false, "Business not found.");

        var cfg = OtpConfig(config);
        if (bp.LastEmailOtpSentAt.HasValue &&
            (DateTime.UtcNow - bp.LastEmailOtpSentAt.Value).TotalSeconds < cfg.CooldownSeconds)
            return (false, $"Please wait {cfg.CooldownSeconds} seconds before resending.");

        return await SendEmailOtpAsync(id, userId, config, email);
    }

    // ── OTP: Mobile ───────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> SendMobileOtpAsync(
        int id, int userId, IConfiguration config, ISmsService sms)
    {
        var bp = await _db.BusinessProfiles.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);
        if (bp is null) return (false, "Business not found.");
        if (bp.IsBusinessMobileVerified) return (false, "Mobile is already verified.");

        var cfg = OtpConfig(config);
        if (bp.MobileResendCount >= cfg.MaxResend)
            return (false, $"Maximum OTP resend limit ({cfg.MaxResend}) reached.");

        var otp = GenerateOtp(cfg.Length);
        bp.BusinessMobileOtpCode = otp;
        bp.BusinessMobileOtpExpiry = DateTime.UtcNow.AddMinutes(cfg.ExpiryMinutes);
        bp.MobileOtpAttempts = 0;
        bp.MobileResendCount++;
        bp.LastMobileOtpSentAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var msg = $"Your SMElevate business verification OTP is {otp}. It is valid for {cfg.ExpiryMinutes} minutes. Do not share this code with anyone.";
        await sms.SendAsync(bp.CellOrLandlineNo, msg);
        _logger.LogInformation("Mobile OTP sent for business {Id} to {Masked}", id, MaskMobile(bp.CellOrLandlineNo));
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> VerifyMobileOtpAsync(
        int id, int userId, string otp, IConfiguration config)
    {
        var bp = await _db.BusinessProfiles.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);
        if (bp is null) return (false, "Business not found.");
        if (bp.IsBusinessMobileVerified) return (true, null);

        var cfg = OtpConfig(config);
        if (bp.MobileOtpAttempts >= cfg.MaxAttempts)
            return (false, "Too many failed attempts. Please request a new OTP.");
        if (string.IsNullOrEmpty(bp.BusinessMobileOtpCode) || bp.BusinessMobileOtpExpiry < DateTime.UtcNow)
            return (false, "OTP has expired. Please request a new one.");
        if (bp.BusinessMobileOtpCode != otp.Trim())
        {
            bp.MobileOtpAttempts++;
            await _db.SaveChangesAsync();
            return (false, "Invalid OTP. Please try again.");
        }
        bp.IsBusinessMobileVerified = true;
        bp.BusinessMobileOtpCode = null;
        bp.BusinessMobileOtpExpiry = null;
        bp.MobileOtpAttempts = 0;
        RecalcVerificationStatus(bp);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Mobile verified for business {Id}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ResendMobileOtpAsync(
        int id, int userId, IConfiguration config, ISmsService sms)
    {
        var bp = await _db.BusinessProfiles.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId && b.IsActive);
        if (bp is null) return (false, "Business not found.");

        var cfg = OtpConfig(config);
        if (bp.LastMobileOtpSentAt.HasValue &&
            (DateTime.UtcNow - bp.LastMobileOtpSentAt.Value).TotalSeconds < cfg.CooldownSeconds)
            return (false, $"Please wait {cfg.CooldownSeconds} seconds before resending.");

        return await SendMobileOtpAsync(id, userId, config, sms);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void RecalcVerificationStatus(BusinessProfile bp)
    {
        if (bp.IsBusinessEmailVerified && bp.IsBusinessMobileVerified)
        {
            bp.BusinessVerificationStatus = Models.Common.BusinessVerificationStatus.Verified;
            bp.BusinessVerifiedAt ??= DateTime.UtcNow;
        }
        else if (bp.IsBusinessEmailVerified || bp.IsBusinessMobileVerified)
            bp.BusinessVerificationStatus = Models.Common.BusinessVerificationStatus.PartiallyVerified;
        else
            bp.BusinessVerificationStatus = Models.Common.BusinessVerificationStatus.Pending;
    }

    private static string GenerateOtp(int length) =>
        new Random(Environment.TickCount).Next(
            (int)Math.Pow(10, length - 1),
            (int)Math.Pow(10, length) - 1).ToString();

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return email;
        return email[0] + new string('*', Math.Min(at - 1, 5)) + email[at..];
    }

    private static string MaskMobile(string mobile) =>
        mobile.Length > 4
            ? mobile[..4] + new string('*', Math.Max(0, mobile.Length - 5)) + mobile[^1]
            : "****";

    private record OtpSettings(int Length, int ExpiryMinutes, int CooldownSeconds, int MaxAttempts, int MaxResend);

    private static OtpSettings OtpConfig(IConfiguration config) => new(
        Length:          config.GetValue("BusinessProfileOtp:Length", 6),
        ExpiryMinutes:   config.GetValue("BusinessProfileOtp:ExpiryMinutes", 5),
        CooldownSeconds: config.GetValue("BusinessProfileOtp:ResendCooldownSeconds", 60),
        MaxAttempts:     config.GetValue("BusinessProfileOtp:MaximumVerificationAttempts", 5),
        MaxResend:       config.GetValue("BusinessProfileOtp:MaximumResendAttempts", 5));
}
