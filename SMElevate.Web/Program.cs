using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using SMElevate.Web.Data;
using SMElevate.Web.Middleware;
using SMElevate.Web.Services.Interfaces;
using SMElevate.Web.Services.Implementations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Cookie Authentication ─────────────────────────────────────────────────
var authBuilder = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    // Intermediate scheme: holds the external identity during the OAuth callback
    .AddCookie("External", o =>
    {
        o.Cookie.Name = ".SMElevate.External";
        o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    });

// ─── External OAuth Providers (only registered when configured) ────────────
var googleClientId     = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(o =>
    {
        o.ClientId     = googleClientId;
        o.ClientSecret = googleClientSecret;
        o.SignInScheme  = "External";
        o.SaveTokens    = false;
    });
}

var msClientId     = builder.Configuration["Authentication:Microsoft:ClientId"];
var msClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
var msTenantId     = builder.Configuration["Authentication:Microsoft:TenantId"] ?? "common";
if (!string.IsNullOrWhiteSpace(msClientId) && !string.IsNullOrWhiteSpace(msClientSecret))
{
    authBuilder.AddMicrosoftAccount(o =>
    {
        o.ClientId     = msClientId;
        o.ClientSecret = msClientSecret;
        o.SignInScheme  = "External";
        o.SaveTokens    = false;
        // Override authority for specific tenant when not "common"
        if (!string.Equals(msTenantId, "common", StringComparison.OrdinalIgnoreCase))
            o.AuthorizationEndpoint = $"https://login.microsoftonline.com/{msTenantId}/oauth2/v2.0/authorize";
    });
}

var appleClientId  = builder.Configuration["Authentication:Apple:ClientId"];
var appleTeamId    = builder.Configuration["Authentication:Apple:TeamId"];
var appleKeyId     = builder.Configuration["Authentication:Apple:KeyId"];
var applePrivKey   = builder.Configuration["Authentication:Apple:PrivateKey"];
if (!string.IsNullOrWhiteSpace(appleClientId) && !string.IsNullOrWhiteSpace(appleTeamId) &&
    !string.IsNullOrWhiteSpace(appleKeyId)    && !string.IsNullOrWhiteSpace(applePrivKey))
{
    authBuilder.AddApple(o =>
    {
        o.ClientId    = appleClientId;
        o.TeamId      = appleTeamId;
        o.KeyId       = appleKeyId;
        o.SignInScheme = "External";
        o.SaveTokens   = false;
        // v10 UsePrivateKey expects Func<string keyId, IFileInfo> — wrap key string in-memory
        o.UsePrivateKey(_ => new AppleInMemoryKeyFileInfo(applePrivKey));
    });
}

// ─── Authorization Policies ────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireClaim("UserType", "Admin"));
    options.AddPolicy("BankOnly", p => p.RequireClaim("UserType", "Bank"));
    options.AddPolicy("EndUserOnly", p => p.RequireClaim("UserType", "SME"));
    options.AddPolicy("AdminOrBank", p => p.RequireClaim("UserType", "Admin", "Bank"));
});

// ─── Application Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<ILoanRequestService, LoanRequestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<ISchemeService, SchemeService>();
builder.Services.AddScoped<ISchemeFormConfigService, SchemeFormConfigService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IRequestNumberService, RequestNumberService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// ─── Business Profile Services ─────────────────────────────────────────────
builder.Services.AddScoped<IBusinessProfileService, BusinessProfileService>();
builder.Services.AddScoped<ISmsService, SmsService>();

// ─── V2.2 Lifecycle and Module Services ───────────────────────────────────
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IBankAssessmentService, BankAssessmentService>();
builder.Services.AddScoped<IAdditionalInfoRequestService, AdditionalInfoRequestService>();
builder.Services.AddScoped<IBankDecisionService, BankDecisionService>();
builder.Services.AddScoped<IConditionalOfferService, ConditionalOfferService>();
builder.Services.AddScoped<IPostApprovalService, PostApprovalService>();
builder.Services.AddScoped<IDisbursementService, DisbursementService>();
builder.Services.AddScoped<IApplicationMonitoringService, ApplicationMonitoringService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ─── MVC with Areas ────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// ─── Session (for OTP temp storage) ───────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddMemoryCache();

var app = builder.Build();

// ─── Error handling ────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

// ─── Middleware pipeline ───────────────────────────────────────────────────
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ─── EndUser: first-login business profile check ───────────────────────────
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/EndUser"),
    branch => branch.UseMiddleware<EndUserBusinessProfileCheckMiddleware>());

// ─── Route configuration ────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "Admin",
    pattern: "Admin/{controller=AdminDashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" },
    constraints: new { area = "Admin" }
).WithMetadata(new Microsoft.AspNetCore.Mvc.AreaAttribute("Admin"));

app.MapControllerRoute(
    name: "Banks",
    pattern: "Banks/{controller=BankDashboard}/{action=Index}/{id?}",
    defaults: new { area = "Banks" },
    constraints: new { area = "Banks" }
).WithMetadata(new Microsoft.AspNetCore.Mvc.AreaAttribute("Banks"));

app.MapControllerRoute(
    name: "EndUser",
    pattern: "EndUser/{controller=EndUserDashboard}/{action=Index}/{id?}",
    defaults: new { area = "EndUser" },
    constraints: new { area = "EndUser" }
).WithMetadata(new Microsoft.AspNetCore.Mvc.AreaAttribute("EndUser"));

app.MapControllerRoute(
    name: "areaDefault",
    pattern: "{area:exists}/{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ─── Database initialization ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
        await DataSeeder.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database initialization failed.");
    }
}

app.Run();

// ─── In-memory IFileInfo for Apple private key ─────────────────────────────
// AspNet.Security.OAuth.Apple v10 UsePrivateKey expects Func<string, IFileInfo>
sealed class AppleInMemoryKeyFileInfo(string keyContent) : IFileInfo
{
    private readonly byte[] _bytes = Encoding.UTF8.GetBytes(keyContent);
    public bool Exists        => true;
    public bool IsDirectory   => false;
    public long Length        => _bytes.Length;
    public string? PhysicalPath => null;
    public string Name        => "apple.p8";
    public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
    public Stream CreateReadStream()   => new MemoryStream(_bytes);
}
