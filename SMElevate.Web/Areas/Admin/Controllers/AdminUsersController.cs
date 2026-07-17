using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMElevate.Web.Areas.Admin.ViewModels;
using SMElevate.Web.Data;
using SMElevate.Web.Models.Common;
using SMElevate.Web.Services.Interfaces;
using System.Text;

namespace SMElevate.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminUsersController : Controller
{
    private readonly IUserService _users;
    private readonly IRoleService _roles;
    private readonly IBankService _banks;
    private readonly IAuditService _audit;
    private readonly IAuthService _auth;
    private readonly ApplicationDbContext _db;

    public AdminUsersController(IUserService users, IRoleService roles, IBankService banks, IAuditService audit, IAuthService auth, ApplicationDbContext db)
    {
        _users = users; _roles = roles; _banks = banks; _audit = audit; _auth = auth; _db = db;
    }

    private void SetNav(string title = "Users")
    {
        ViewData["ActiveNav"] = "Users";
        ViewData["PreTitle"] = "User Management";
        ViewData["Title"] = title;
    }

    public async Task<IActionResult> Index(int? roleId = null, string? userType = null, string? status = null, string? search = null)
    {
        SetNav();
        var allUsers = await _users.GetAllUsersAsync();
        return View(new UserListViewModel
        {
            Users = ApplyFilters(allUsers, roleId, userType, status, search),
            Roles = await _roles.GetAllRolesAsync(),
            FilterRoleId = roleId,
            FilterUserType = userType,
            FilterStatus = status,
            FilterSearch = search
        });
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(int? roleId = null, string? userType = null, string? status = null, string? search = null)
    {
        var allUsers = await _users.GetAllUsersAsync();
        var filtered = ApplyFilters(allUsers, roleId, userType, status, search);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        sb.AppendLine("<Styles><Style ss:ID=\"H\"><Font ss:Bold=\"1\"/></Style></Styles>");
        sb.AppendLine("<Worksheet ss:Name=\"Users\"><Table>");
        sb.AppendLine("<Row ss:StyleID=\"H\">");
        foreach (var h in new[] { "Full Name", "Email", "Mobile No", "User Type", "Role", "Bank", "Auth Mode", "Status", "Created At", "Last Login" })
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(h)}</Data></Cell>");
        sb.AppendLine("</Row>");

        foreach (var u in filtered)
        {
            sb.AppendLine("<Row>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.FullName)}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.EmailAddress)}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.MobileNo ?? "")}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.UserType.ToString())}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.Role?.RoleName ?? "")}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.Bank?.BankName ?? "")}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Enc(u.AuthenticationMode ?? "")}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{(u.IsActive ? "Active" : "Inactive")}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{u.CreatedAt:yyyy-MM-dd}</Data></Cell>");
            sb.AppendLine($"<Cell><Data ss:Type=\"String\">{u.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? ""}</Data></Cell>");
            sb.AppendLine("</Row>");
        }
        sb.AppendLine("</Table></Worksheet></Workbook>");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", $"users_{DateTime.UtcNow:yyyyMMdd}.xls");
    }

    public async Task<IActionResult> Detail(int id)
    {
        SetNav("User Detail");
        var user = await _users.GetUserByIdAsync(id);
        if (user is null) return NotFound();
        return View(new UserDetailViewModel { User = user, Profile = user.Profile });
    }

    public async Task<IActionResult> Create()
    {
        SetNav("New User");
        return View(new UserFormViewModel
        {
            Roles = await _roles.GetAllRolesAsync(),
            Banks = await _banks.GetAllBanksAsync(activeOnly: true),
            IsActive = true
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel vm)
    {
        // Conditional password validation for SME users
        if (vm.UserType == UserType.SME)
        {
            if (string.IsNullOrEmpty(vm.NewPassword))
                ModelState.AddModelError("NewPassword", "Password is required for SME users.");
            else if (!IsStrongPassword(vm.NewPassword))
                ModelState.AddModelError("NewPassword", "Password must be at least 8 characters with uppercase, lowercase, number, and special character.");
            else if (vm.NewPassword != vm.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
        }
        else if (vm.UserType != UserType.Bank && !string.IsNullOrEmpty(vm.NewPassword) && vm.NewPassword != vm.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
        }

        if (!ModelState.IsValid)
        {
            vm.Roles = await _roles.GetAllRolesAsync(); vm.Banks = await _banks.GetAllBanksAsync(true);
            return View(vm);
        }

        if (await _users.EmailExistsAsync(vm.EmailAddress))
        {
            ModelState.AddModelError("EmailAddress", "This email address is already registered.");
            vm.Roles = await _roles.GetAllRolesAsync(); vm.Banks = await _banks.GetAllBanksAsync(true);
            return View(vm);
        }

        var authMode = vm.UserType switch
        {
            UserType.SME => "Manual",
            UserType.Bank => "OTP",
            _ => string.IsNullOrEmpty(vm.AuthenticationMode) ? "Manual" : vm.AuthenticationMode
        };

        var user = new ApplicationUser
        {
            FullName = vm.FullName, EmailAddress = vm.EmailAddress, MobileNo = vm.MobileNo,
            UserType = vm.UserType, RoleId = vm.RoleId, BankId = vm.BankId, IsActive = vm.IsActive,
            AuthenticationMode = authMode
        };

        // Bank users use OTP — no manual password needed; generate a secure internal one
        var password = vm.UserType == UserType.Bank
            ? Guid.NewGuid().ToString("N")[..12] + "Aa1!"
            : (vm.NewPassword ?? "Temp@12345");

        await _users.CreateUserAsync(user, password);
        await _audit.LogAsync("Create", "ApplicationUser", user.Id, newValue: vm.EmailAddress);

        // Auto-assign Bank user as BankMember
        if (vm.UserType == UserType.Bank && vm.BankId.HasValue)
        {
            _db.BankMembers.Add(new BankMember { UserId = user.Id, BankId = vm.BankId.Value, IsActive = true, AssignedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            var bank = await _db.Banks.AsNoTracking().FirstOrDefaultAsync(b => b.Id == vm.BankId);
            TempData["BankMemberAdded"] = $"{user.FullName} has been added as a member of {bank?.BankName ?? "the selected bank"}. They can log in using OTP.";
        }

        TempData["Success"] = "User created successfully.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        SetNav("Edit User");
        var user = await _users.GetUserByIdAsync(id);
        if (user is null) return NotFound();
        return View(new UserFormViewModel
        {
            Id = user.Id, FullName = user.FullName, EmailAddress = user.EmailAddress, MobileNo = user.MobileNo,
            UserType = user.UserType, RoleId = user.RoleId, BankId = user.BankId, IsActive = user.IsActive,
            AuthenticationMode = user.AuthenticationMode,
            Roles = await _roles.GetAllRolesAsync(), Banks = await _banks.GetAllBanksAsync(true)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
    {
        // Validate new password only if provided
        if (!string.IsNullOrEmpty(vm.NewPassword))
        {
            if (vm.UserType == UserType.SME && !IsStrongPassword(vm.NewPassword))
                ModelState.AddModelError("NewPassword", "Password must be at least 8 characters with uppercase, lowercase, number, and special character.");
            else if (vm.NewPassword != vm.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
        }

        if (!ModelState.IsValid) { vm.Roles = await _roles.GetAllRolesAsync(); vm.Banks = await _banks.GetAllBanksAsync(true); return View(vm); }

        var user = await _users.GetUserByIdAsync(id);
        if (user is null) return NotFound();

        if (await _users.EmailExistsAsync(vm.EmailAddress, id))
        { ModelState.AddModelError("EmailAddress", "Email already in use."); vm.Roles = await _roles.GetAllRolesAsync(); vm.Banks = await _banks.GetAllBanksAsync(true); return View(vm); }

        var authMode = vm.UserType switch
        {
            UserType.SME => "Manual",
            UserType.Bank => "OTP",
            _ => string.IsNullOrEmpty(vm.AuthenticationMode) ? (user.AuthenticationMode ?? "Manual") : vm.AuthenticationMode
        };

        user.FullName = vm.FullName; user.EmailAddress = vm.EmailAddress; user.MobileNo = vm.MobileNo;
        user.UserType = vm.UserType; user.RoleId = vm.RoleId; user.BankId = vm.BankId; user.IsActive = vm.IsActive;
        user.AuthenticationMode = authMode;

        if (!string.IsNullOrEmpty(vm.NewPassword))
            user.PasswordHash = _auth.HashPassword(vm.NewPassword);

        await _users.UpdateUserAsync(user);
        await _audit.LogAsync("Update", "ApplicationUser", id, newValue: vm.EmailAddress);
        TempData["Success"] = "User updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var newState = await _users.ActivateDeactivateAsync(id);
        await _audit.LogAsync(newState ? "Activate" : "Deactivate", "ApplicationUser", id);
        TempData["Success"] = newState ? "User activated." : "User deactivated.";
        return RedirectToAction("Index");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static List<ApplicationUser> ApplyFilters(List<ApplicationUser> users, int? roleId, string? userType, string? status, string? search)
    {
        var q = users.AsEnumerable();
        if (roleId.HasValue) q = q.Where(u => u.RoleId == roleId);
        if (!string.IsNullOrEmpty(userType)) q = q.Where(u => u.UserType.ToString() == userType);
        if (status == "active") q = q.Where(u => u.IsActive);
        else if (status == "inactive") q = q.Where(u => !u.IsActive);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            q = q.Where(u => u.FullName.ToLower().Contains(s) || u.EmailAddress.ToLower().Contains(s));
        }
        return q.ToList();
    }

    private static bool IsStrongPassword(string pwd) =>
        pwd.Length >= 8 &&
        pwd.Any(char.IsUpper) &&
        pwd.Any(char.IsLower) &&
        pwd.Any(char.IsDigit) &&
        pwd.Any(c => "!@#$%^&*()_+-=[]{}|;':\",./<>?".Contains(c));

    private static string Enc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
