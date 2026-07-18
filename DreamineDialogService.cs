namespace Dreamine.UI.Blazor;

/// <summary>
/// \if KO
/// <para>현재 대화 상자 요청을 보관하고 호스트 컴포넌트에 상태 변경을 알리는 Blazor 팝업 서비스입니다.</para>
/// \endif
/// \if EN
/// <para>Provides a Blazor popup service that stores the current dialog request and notifies a host component of state changes.</para>
/// \endif
/// </summary>
/// <remarks>
/// \if KO
/// <para>Blazor에는 별도의 운영체제 창이 없으므로 페이지에 배치된 DreamineDialogHost가 <see cref="RequestChanged"/>를 구독하여 오버레이를 렌더링합니다. DI 컨테이너에는 scoped 수명으로 등록하는 것이 적합합니다.</para>
/// \endif
/// \if EN
/// <para>Because Blazor has no separate operating-system windows, a DreamineDialogHost placed on the page subscribes to <see cref="RequestChanged"/> and renders an overlay. A scoped DI lifetime is appropriate.</para>
/// \endif
/// </remarks>
public sealed class DreamineDialogService
{
    /// <summary>
    /// \if KO
    /// <para>호스트 컴포넌트가 렌더링할 대화 상자의 불변 구성과 런타임 카운트다운 상태를 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Represents the immutable configuration and runtime countdown state of a dialog rendered by the host component.</para>
    /// \endif
    /// </summary>
    public sealed class DialogRequest
    {
        /// <summary>
        /// \if KO
        /// <para>대화 상자 제목을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the dialog title.</para>
        /// \endif
        /// </summary>
        public required string Title { get; init; }
        /// <summary>
        /// \if KO
        /// <para>대화 상자 메시지를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the dialog message.</para>
        /// \endif
        /// </summary>
        public required string Message { get; init; }
        /// <summary>
        /// \if KO
        /// <para>결과 값과 표시 텍스트로 구성된 버튼 목록을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the button list composed of result values and display text.</para>
        /// \endif
        /// </summary>
        public required IReadOnlyList<(DreamineDialogResult Result, string Text)> Buttons { get; init; }
        /// <summary>
        /// \if KO
        /// <para>배경 깜빡임 효과를 사용할지 여부를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets whether the background blinking effect is enabled.</para>
        /// \endif
        /// </summary>
        public bool UseBlink { get; init; }
        /// <summary>
        /// \if KO
        /// <para>첫 번째 배경 CSS 색상을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the first background CSS color.</para>
        /// \endif
        /// </summary>
        public string Color1 { get; init; } = "#0F1E3A";
        /// <summary>
        /// \if KO
        /// <para>두 번째 배경 CSS 색상을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the second background CSS color.</para>
        /// \endif
        /// </summary>
        public string Color2 { get; init; } = "#0F1E3A";
        /// <summary>
        /// \if KO
        /// <para>콘텐츠의 CSS 전경색을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the CSS foreground color of the content.</para>
        /// \endif
        /// </summary>
        public string ForegroundColor { get; init; } = "#FFFFFF";
        /// <summary>
        /// \if KO
        /// <para>깜빡임 전환 간격을 밀리초 단위로 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the blink transition interval in milliseconds.</para>
        /// \endif
        /// </summary>
        public int BlinkIntervalMs { get; init; } = 600;
        /// <summary>
        /// \if KO
        /// <para>자동 선택까지 남은 초를 가져오거나 설정합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets or sets the number of seconds remaining before automatic selection.</para>
        /// \endif
        /// </summary>
        public int AutoClickRemainingSeconds { get; set; }
        /// <summary>
        /// \if KO
        /// <para>카운트다운이 끝날 때 자동으로 선택할 결과를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the result selected automatically when the countdown expires.</para>
        /// \endif
        /// </summary>
        public DreamineDialogResult AutoClickResult { get; init; }
    }

    /// <summary>
    /// \if KO
    /// <para>tcs 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the tcs value.</para>
    /// \endif
    /// </summary>
    private TaskCompletionSource<DreamineDialogResult>? _tcs;

    /// <summary>
    /// \if KO
    /// <para>현재 표시 중인 대화 상자 요청을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the dialog request currently being displayed.</para>
    /// \endif
    /// </summary>
    public DialogRequest? Current { get; private set; }

    /// <summary>
    /// \if KO
    /// <para>현재 대화 상자 요청 또는 카운트다운 상태가 변경될 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when the current dialog request or countdown state changes.</para>
    /// \endif
    /// </summary>
    public event Action? RequestChanged;

    /// <summary>
    /// \if KO
    /// <para>확인 버튼이 있는 정보 대화 상자를 비동기로 표시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously displays an informational dialog with an OK button.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>표시할 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message to display.</para>
    /// \endif
    /// </param>
    /// <param name="title">
    /// \if KO
    /// <para>대화 상자 제목입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The dialog title.</para>
    /// \endif
    /// </param>
    /// <param name="autoClickDelaySeconds">
    /// \if KO
    /// <para>확인 버튼을 자동 선택하기 전 대기할 초입니다. 0 이하는 자동 선택을 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of seconds before automatically selecting OK. A non-positive value disables automatic selection.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>사용자가 선택하거나 자동 선택된 결과를 생성하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that produces the user-selected or automatically selected result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>확인 및 취소 버튼이 있는 확인 대화 상자를 비동기로 표시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously displays a confirmation dialog with OK and Cancel buttons.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>표시할 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message to display.</para>
    /// \endif
    /// </param>
    /// <param name="title">
    /// \if KO
    /// <para>대화 상자 제목입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The dialog title.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>사용자가 선택한 결과를 생성하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that produces the result selected by the user.</para>
    /// \endif
    /// </returns>
    public Task<DreamineDialogResult> ShowOkCancelAsync(string message, string title = "Confirm")
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Buttons = new[] { (DreamineDialogResult.Cancel, "취소"), (DreamineDialogResult.OK, "확인") }
        });

    /// <summary>
    /// \if KO
    /// <para>예 및 아니요 버튼이 있는 질문 대화 상자를 비동기로 표시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously displays a question dialog with Yes and No buttons.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>표시할 질문입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The question to display.</para>
    /// \endif
    /// </param>
    /// <param name="title">
    /// \if KO
    /// <para>대화 상자 제목입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The dialog title.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>사용자가 선택한 결과를 생성하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that produces the result selected by the user.</para>
    /// \endif
    /// </returns>
    public Task<DreamineDialogResult> ShowYesNoAsync(string message, string title = "Question")
        => Show(new DialogRequest
        {
            Title = title,
            Message = message,
            Buttons = new[] { (DreamineDialogResult.No, "아니오"), (DreamineDialogResult.Yes, "예") }
        });

    /// <summary>
    /// \if KO
    /// <para>지정한 옵션으로 깜빡임 대화 상자를 비동기로 표시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously displays a blinking dialog using the specified options.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>대화 상자의 콘텐츠와 표시 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The content and display options for the dialog.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>사용자가 선택한 결과를 생성하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that produces the result selected by the user.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>요청을 현재 대화 상자로 게시하고 완료 결과 작업을 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Publishes a request as the current dialog and returns its completion task.</para>
    /// \endif
    /// </summary>
    /// <param name="request">
    /// \if KO
    /// <para>게시할 대화 상자 요청입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The dialog request to publish.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>대화 상자가 완료될 때 결과를 생성하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that produces the result when the dialog completes.</para>
    /// \endif
    /// </returns>
    private Task<DreamineDialogResult> Show(DialogRequest request)
    {
        _tcs = new TaskCompletionSource<DreamineDialogResult>();
        Current = request;
        RequestChanged?.Invoke();
        return _tcs.Task;
    }

    /// <summary>
    /// \if KO
    /// <para>현재 대화 상자를 지정한 결과로 완료하고 대기 중인 작업을 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Completes the current dialog with the specified result and releases the pending task.</para>
    /// \endif
    /// </summary>
    /// <param name="result">
    /// \if KO
    /// <para>대화 상자의 최종 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The final result of the dialog.</para>
    /// \endif
    /// </param>
    public void Complete(DreamineDialogResult result)
    {
        Current = null;
        RequestChanged?.Invoke();
        _tcs?.TrySetResult(result);
        _tcs = null;
    }

    /// <summary>
    /// \if KO
    /// <para>자동 선택 카운트다운을 1초 줄이고 만료 시 구성된 결과로 대화 상자를 완료합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Decrements the automatic-selection countdown by one second and completes the dialog with the configured result when it expires.</para>
    /// \endif
    /// </summary>
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
