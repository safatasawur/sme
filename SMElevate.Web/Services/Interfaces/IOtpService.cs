namespace SMElevate.Web.Services.Interfaces;

public interface IOtpService
{
    string GenerateOtp();
    void StoreOtp(string email, string otp);
    bool ValidateOtp(string email, string otp);
    void ClearOtp(string email);
}
