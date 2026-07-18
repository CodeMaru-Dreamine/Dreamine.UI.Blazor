namespace Dreamine.UI.Blazor;

/// <summary>
/// \if KO
/// <para>Dreamine 버튼의 의미 기반 색상 변형을 지정합니다.</para>
/// \endif
/// \if EN
/// <para>Specifies semantic color variants for a Dreamine button.</para>
/// \endif
/// </summary>
public enum DreamineButtonVariant
{
    /// <summary>
    /// \if KO
    /// <para>기본 버튼 스타일을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the default button style.</para>
    /// \endif
    /// </summary>
    Default,
    /// <summary>
    /// \if KO
    /// <para>주요 동작 버튼 스타일을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the primary-action button style.</para>
    /// \endif
    /// </summary>
    Primary,
    /// <summary>
    /// \if KO
    /// <para>성공 동작 버튼 스타일을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the success-action button style.</para>
    /// \endif
    /// </summary>
    Success,
    /// <summary>
    /// \if KO
    /// <para>경고 동작 버튼 스타일을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the warning-action button style.</para>
    /// \endif
    /// </summary>
    Warning,
    /// <summary>
    /// \if KO
    /// <para>위험 동작 버튼 스타일을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the dangerous-action button style.</para>
    /// \endif
    /// </summary>
    Danger,
    /// <summary>
    /// \if KO
    /// <para>배경이 강조되지 않는 고스트 버튼 스타일을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses a low-emphasis ghost button style.</para>
    /// \endif
    /// </summary>
    Ghost
}

/// <summary>
/// \if KO
/// <para>Dreamine 버튼의 크기 변형을 지정합니다.</para>
/// \endif
/// \if EN
/// <para>Specifies size variants for a Dreamine button.</para>
/// \endif
/// </summary>
public enum DreamineButtonSize
{
    /// <summary>
    /// \if KO
    /// <para>작은 버튼 크기를 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the small button size.</para>
    /// \endif
    /// </summary>
    Small,
    /// <summary>
    /// \if KO
    /// <para>기본 버튼 크기를 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the normal button size.</para>
    /// \endif
    /// </summary>
    Normal,
    /// <summary>
    /// \if KO
    /// <para>큰 버튼 크기를 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses the large button size.</para>
    /// \endif
    /// </summary>
    Large
}
