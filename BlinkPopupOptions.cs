namespace Dreamine.UI.Blazor;

/// <summary>
/// WPF Dreamine.UI.Abstractions.Popup.BlinkPopupOptions / WinForms·MAUI의
/// BlinkPopupOptions에 대응하는 Blazor 전용 옵션.
/// </summary>
public sealed class BlinkPopupOptions
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? OkText { get; set; }
    public string? CancelText { get; set; }
    public bool UseBlink { get; set; } = true;
    public string Color1 { get; set; } = "#B41E1E";
    public string Color2 { get; set; } = "#500A0A";
    public string ForegroundColor { get; set; } = "#FFD700";
    public int BlinkIntervalMs { get; set; } = 600;
}
