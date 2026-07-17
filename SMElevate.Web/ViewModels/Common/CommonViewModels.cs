namespace SMElevate.Web.ViewModels.Common;

public class AlertViewModel
{
    public string Type { get; set; } = "info"; // success, danger, warning, info
    public string Message { get; set; } = default!;
}
