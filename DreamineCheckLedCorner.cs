namespace Dreamine.UI.Blazor;

/// <summary>
/// \if KO
/// <para>체크 LED를 배치할 네 모서리 앵커를 지정합니다.</para>
/// \endif
/// \if EN
/// <para>Specifies one of four corner anchors for positioning a check LED.</para>
/// \endif
/// </summary>
public enum DreamineCheckLedCorner
{
    /// <summary>
    /// \if KO
    /// <para>왼쪽 위 모서리에 배치합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Positions the LED at the top-left corner.</para>
    /// \endif
    /// </summary>
    TopLeft,
    /// <summary>
    /// \if KO
    /// <para>오른쪽 위 모서리에 배치합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Positions the LED at the top-right corner.</para>
    /// \endif
    /// </summary>
    TopRight,
    /// <summary>
    /// \if KO
    /// <para>왼쪽 아래 모서리에 배치합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Positions the LED at the bottom-left corner.</para>
    /// \endif
    /// </summary>
    BottomLeft,
    /// <summary>
    /// \if KO
    /// <para>오른쪽 아래 모서리에 배치합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Positions the LED at the bottom-right corner.</para>
    /// \endif
    /// </summary>
    BottomRight
}
