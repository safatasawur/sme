using SMElevate.Web.Services.Interfaces;

namespace SMElevate.Web.Middleware;

public class EndUserBusinessProfileCheckMiddleware
{
    private readonly RequestDelegate _next;

    // Paths exempt from business profile check
    private static readonly HashSet<string> _exemptControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "BusinessProfiles", "EndUserAccount", "EndUserProfile", "EndUserNotifications"
    };
    private static readonly HashSet<string> _exemptActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Create", "Verify", "VerifyEmailOtp", "VerifyMobileOtp",
        "ResendEmailOtp", "ResendMobileOtp", "Logout", "Error"
    };

    public EndUserBusinessProfileCheckMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to authenticated EndUsers inside the EndUser area
        if (!context.User.Identity?.IsAuthenticated == true ||
            context.User.FindFirst("UserType")?.Value != "SME")
        {
            await _next(context);
            return;
        }

        var routeData = context.GetRouteData();
        var area = routeData?.Values["area"]?.ToString();
        if (!string.Equals(area, "EndUser", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var controller = routeData?.Values["controller"]?.ToString() ?? "";
        var action = routeData?.Values["action"]?.ToString() ?? "";

        if (_exemptControllers.Contains(controller) || _exemptActions.Contains(action))
        {
            await _next(context);
            return;
        }

        var uidClaim = context.User.FindFirst("UserId")?.Value;
        if (!int.TryParse(uidClaim, out var userId))
        {
            await _next(context);
            return;
        }

        var bp = context.RequestServices.GetRequiredService<IBusinessProfileService>();

        if (!await bp.UserHasAnyActiveBusinessAsync(userId))
        {
            // No business at all — redirect to create
            context.Response.Redirect("/EndUser/BusinessProfiles/Create");
            return;
        }

        if (!await bp.UserHasVerifiedBusinessAsync(userId))
        {
            // Has business but not yet verified — redirect to verify
            var pending = await bp.GetFirstPendingVerificationAsync(userId);
            if (pending is not null)
            {
                context.Response.Redirect($"/EndUser/BusinessProfiles/Verify/{pending.Id}");
                return;
            }
        }

        await _next(context);
    }
}
