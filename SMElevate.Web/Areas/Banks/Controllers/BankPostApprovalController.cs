using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.Banks.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Areas.Banks.Controllers;

[Area("Banks")]
[Authorize(Policy = "BankOnly")]
public class BankPostApprovalController : Controller
{
    private readonly IPostApprovalService _postApproval;
    private readonly IWorkflowService _workflow;
    private readonly IAuditService _audit;
    private readonly INotificationService _notifications;
    private readonly ApplicationDbContext _db;

    public BankPostApprovalController(IPostApprovalService postApproval, IWorkflowService workflow,
        IAuditService audit, INotificationService notifications, ApplicationDbContext db)
    { _postApproval = postApproval; _workflow = workflow; _audit = audit; _notifications = notifications; _db = db; }

    private int BankId => int.TryParse(User.FindFirst("BankId")?.Value, out var id) ? id : 0;
    private int UserId => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Index(int loanRequestId)
    {
        var request = await _db.LoanRequests.Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        ViewData["Title"] = "Post-Approval Checklist"; ViewData["PreTitle"] = "Post-Approval"; ViewData["ActiveNav"] = "Requests";
        ViewBag.Request = request;
        ViewBag.Checklist = await _postApproval.GetChecklistByLoanRequestAsync(loanRequestId);
        return View(new CreateChecklistViewModel { LoanRequestId = loanRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateChecklist(CreateChecklistViewModel vm)
    {
        var request = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == vm.LoanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        var checklist = new PostApprovalChecklist
        { LoanRequestId = vm.LoanRequestId, Title = vm.Title, Description = vm.Description, CreatedByUserId = UserId };
        var items = vm.Items.Where(i => !string.IsNullOrWhiteSpace(i.ItemName))
            .Select(i => new PostApprovalChecklistItem
            { ItemName = i.ItemName, ItemDescription = i.ItemDescription, IsRequired = i.IsRequired, DocumentRequired = i.DocumentRequired, DueDate = i.DueDate })
            .ToList();

        await _postApproval.CreateChecklistAsync(checklist, items);
        await _audit.LogAsync("ChecklistCreated", "PostApprovalChecklist", checklist.Id, userId: UserId);
        TempData["Success"] = "Post-approval checklist created.";
        return RedirectToAction("Index", new { loanRequestId = vm.LoanRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyItem(int itemId, string status, string? remarks)
    {
        var item = await _postApproval.GetItemByIdAsync(itemId);
        if (item == null) return NotFound();
        var loanRequestId = item.Checklist.LoanRequestId;

        var request = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        await _postApproval.VerifyItemAsync(itemId, status, UserId, remarks);
        await _audit.LogAsync("ChecklistItemVerified", "PostApprovalChecklistItem", itemId,
            newValue: status, userId: UserId);
        TempData["Success"] = $"Item marked as {status}.";
        return RedirectToAction("Index", new { loanRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AdvanceStatus(int loanRequestId, string toStatus)
    {
        var request = await _db.LoanRequests.Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == loanRequestId && r.AssignedBankId == BankId);
        if (request == null) return NotFound();

        try
        {
            await _workflow.AdvanceStatusAsync(loanRequestId, toStatus, UserId, "Bank");
            TempData["Success"] = $"Status advanced to '{toStatus}'.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Index", new { loanRequestId });
    }
}
