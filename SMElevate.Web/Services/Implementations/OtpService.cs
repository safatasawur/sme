using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Data;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Services.Implementations;

public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public OtpService(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public void StoreOtp(string email, string otp)
    {
        var expiry = _config.GetValue<int>("AppSettings:OtpExpiryMinutes", 10);
        var user = _db.Users.FirstOrDefault(u => u.EmailAddress.ToLower() == email.ToLower());
        if (user is not null)
        {
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(expiry);
            _db.SaveChanges();
        }
    }

    public bool ValidateOtp(string email, string otp)
    {
        var user = _db.Users.FirstOrDefault(u => u.EmailAddress.ToLower() == email.ToLower());
        if (user is null || string.IsNullOrEmpty(user.OtpCode)) return false;
        if (user.OtpExpiry.HasValue && user.OtpExpiry.Value < DateTime.UtcNow) return false;
        return user.OtpCode == otp;
    }

    public void ClearOtp(string email)
    {
        var user = _db.Users.FirstOrDefault(u => u.EmailAddress.ToLower() == email.ToLower());
        if (user is not null)
        {
            user.OtpCode = null;
            user.OtpExpiry = null;
            _db.SaveChanges();
        }
    }
}
