namespace SMElevate.Web.Models.Common;

public class Bank
{
    public int Id { get; set; }
    public string BankName { get; set; } = default!;
    public string IBANPrefix { get; set; } = default!;
    public string? BankCode { get; set; }
    public string BankEmailAddress { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<BankMember> Members { get; set; } = new List<BankMember>();
    public ICollection<LoanRequest> AssignedRequests { get; set; } = new List<LoanRequest>();
}
