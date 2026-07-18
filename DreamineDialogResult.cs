namespace Dreamine.UI.Blazor;

/// <summary>
/// \if KO
/// <para>플랫폼 간 일관된 Blazor 대화 상자 결과를 지정합니다.</para>
/// \endif
/// \if EN
/// <para>Specifies platform-consistent results for Blazor dialogs.</para>
/// \endif
/// </summary>
public enum DreamineDialogResult
{
    /// <summary>
    /// \if KO
    /// <para>선택된 결과가 없음을 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Indicates that no result was selected.</para>
    /// \endif
    /// </summary>
    None,
    /// <summary>
    /// \if KO
    /// <para>사용자가 확인을 선택했음을 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Indicates that the user selected OK.</para>
    /// \endif
    /// </summary>
    OK,
    /// <summary>
    /// \if KO
    /// <para>사용자가 취소를 선택했음을 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Indicates that the user selected Cancel.</para>
    /// \endif
    /// </summary>
    Cancel,
    /// <summary>
    /// \if KO
    /// <para>사용자가 예를 선택했음을 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Indicates that the user selected Yes.</para>
    /// \endif
    /// </summary>
    Yes,
    /// <summary>
    /// \if KO
    /// <para>사용자가 아니요를 선택했음을 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Indicates that the user selected No.</para>
    /// \endif
    /// </summary>
    No
}
