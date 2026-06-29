namespace Dreamine.UI.Blazor;

/// <summary>
/// WPF Dreamine.UI.Abstractions.Popup.IPopupService / WinForms DreamineMessageBox·
/// DreamineBlinkPopup / MAUI의 동명 클래스와 같은 역할을 하는 Blazor용 팝업 서비스.
///
/// Blazor에는 별도의 OS 창이 없으므로, 이 서비스는 "현재 표시할 팝업 요청"을
/// 상태로 들고 있다가 <see cref="RequestChanged"/> 이벤트로 알리고, 페이지 어딘가에
/// 한 번 배치된 <c>DreamineDialogHost</c> 컴포넌트가 그 상태를 구독해서 실제로
/// 오버레이를 그린다(WinForms/MAUI의 모달 창/페이지 대신 컴포넌트 오버레이로 흉내).
/// DI 컨테이너에 Scoped로 등록해서 쓴다.
/// </summary>
public sealed class DreamineDialogService
{
    public sealed class DialogRequest
    {
        public required string Title { get; init; }
        public required string Message { get; init; }
        public required IReadOnlyList<(DreamineDialogResult Result, string Text)> Buttons { get; init; }
        public bool UseBlink { get; init; }
        public string Color1 { get; init; } = "#0F1E3A";
        public string Color2 { get; init; } = "#0F1E3A";
        public string ForegroundColor { get; init; } = "#FFFFFF";
        public int BlinkIntervalMs { get; init; } = 600;
        public int AutoClickRemainingSeconds { get; set; }
        public DreamineDialogResult AutoClickResult { get; init; }
    }

    private TaskCompletionSource<DreamineDialogResult>? _tcs;

    public DialogRequest? Current { get; private set; }

    public event Action? RequestChanged;

    public Task<DreamineDialogResult> ShowMessageBoxAsync(
        string message, string title = "Information", int autoClickDelaySeconds = 0)
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Buttons = new[] { (DreamineDialogResult.OK, "확인") },
            AutoClickRemainingSeconds = autoClickDelaySeconds,
            AutoClickResult = DreamineDialogResult.OK
        });

    public Task<DreamineDialogResult> ShowOkCancelAsync(string message, string title = "Confirm")
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Buttons = new[] { (DreamineDialogResult.Cancel, "취소"), (DreamineDialogResult.OK, "확인") }
        });

    public Task<DreamineDialogResult> ShowYesNoAsync(string message, string title = "Question")
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Buttons = new[] { (DreamineDialogResult.No, "아니오"), (DreamineDialogResult.Yes, "예") }
        });

    public Task<DreamineDialogResult> ShowBlinkAsync(BlinkPopupOptions options)
    {
        var buttons = new List<(DreamineDialogResult, string)>();
        if (!string.IsNullOrEmpty(options.CancelText))
            buttons.Add((DreamineDialogResult.Cancel, options.CancelText));
        if (!string.IsNullOrEmpty(options.OkText))
            buttons.Add((DreamineDialogResult.OK, options.OkText));

        return Show(new DialogRequest
        {
            Title = options.Title ?? string.Empty,
            Message = options.Message ?? string.Empty,
            Buttons = buttons,
            UseBlink = options.UseBlink,
            Color1 = options.Color1,
            Color2 = options.Color2,
            ForegroundColor = options.ForegroundColor,
            BlinkIntervalMs = options.BlinkIntervalMs
        });
    }

    private Task<DreamineDialogResult> Show(DialogRequest request)
    {
        _tcs = new TaskCompletionSource<DreamineDialogResult>();
        Current = request;
        RequestChanged?.Invoke();
        return _tcs.Task;
    }

    /// <summary>DreamineDialogHost가 버튼 클릭이나 자동닫힘 타이머에서 호출한다.</summary>
    public void Complete(DreamineDialogResult result)
    {
        Current = null;
        RequestChanged?.Invoke();
        _tcs?.TrySetResult(result);
        _tcs = null;
    }

    /// <summary>매초 호출해서 자동닫힘 카운트다운을 갱신한다.</summary>
    public void TickAutoClick()
    {
        if (Current is null || Current.AutoClickRemainingSeconds <= 0)
            return;

        Current.AutoClickRemainingSeconds--;
        if (Current.AutoClickRemainingSeconds <= 0)
            Complete(Current.AutoClickResult);
        else
            RequestChanged?.Invoke();
    }
}
