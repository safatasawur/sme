namespace SMElevate.Web.Models.Common;

public class BankMember
{
    public int Id { get; set; }
    public int BankId { get; set; }
    public int UserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public Bank Bank { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}
