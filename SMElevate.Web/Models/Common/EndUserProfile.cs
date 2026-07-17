namespace SMElevate.Web.Models.Common;

public class EndUserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string MobileNo { get; set; } = default!;
    public string CNIC { get; set; } = default!;
    public string BusinessEmailAddress { get; set; } = default!;
    public string GenderOfProprietor { get; set; } = default!;
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
