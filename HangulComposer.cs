namespace Dreamine.UI.Blazor;

/// <summary>
/// \if KO
/// <para>가상 키보드에서 입력한 한글 자모를 표준 완성형 음절로 조합합니다.</para>
/// \endif
/// \if EN
/// <para>Composes Hangul jamo entered through a virtual keyboard into standard precomposed syllables.</para>
/// \endif
/// </summary>
/// <remarks>
/// \if KO
/// <para>브라우저 환경에서 운영체제 IME에 의존하지 않고 초성, 중성 및 종성 상태를 직접 관리합니다.</para>
/// \endif
/// \if EN
/// <para>Manages initial, medial, and final consonant state directly without relying on an operating-system IME in the browser.</para>
/// \endif
/// </remarks>
internal sealed class HangulComposer
{
    /// <summary>
    /// \if KO
    /// <para>Cho 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the cho value.</para>
    /// \endif
    /// </summary>
    private static readonly string[] Cho =
    [
        "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ",
        "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ"
    ];

    /// <summary>
    /// \if KO
    /// <para>Jung 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the jung value.</para>
    /// \endif
    /// </summary>
    private static readonly string[] Jung =
    [
        "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ",
        "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ"
    ];

    /// <summary>
    /// \if KO
    /// <para>Jong 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the jong value.</para>
    /// \endif
    /// </summary>
    private static readonly string[] Jong =
    [
        "", "ㄱ", "ㄲ", "ㄳ", "ㄴ", "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ",
        "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ",
        "ㅆ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ"
    ];

    /// <summary>
    /// \if KO
    /// <para>Cho Index 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the cho index value.</para>
    /// \endif
    /// </summary>
    private static readonly Dictionary<string, int> ChoIndex = Cho
        .Select((value, index) => (value, index))
        .ToDictionary(x => x.value, x => x.index);

    /// <summary>
    /// \if KO
    /// <para>Jung Index 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the jung index value.</para>
    /// \endif
    /// </summary>
    private static readonly Dictionary<string, int> JungIndex = Jung
        .Select((value, index) => (value, index))
        .ToDictionary(x => x.value, x => x.index);

    /// <summary>
    /// \if KO
    /// <para>Jong Index 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the jong index value.</para>
    /// \endif
    /// </summary>
    private static readonly Dictionary<string, int> JongIndex = Jong
        .Select((value, index) => (value, index))
        .Where(x => x.value.Length > 0)
        .ToDictionary(x => x.value, x => x.index);

    /// <summary>
    /// \if KO
    /// <para>Combined Jung 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the combined jung value.</para>
    /// \endif
    /// </summary>
    private static readonly Dictionary<(int First, int Second), int> CombinedJung = new()
    {
        [(8, 0)] = 9,
        [(8, 1)] = 10,
        [(8, 20)] = 11,
        [(13, 4)] = 14,
        [(13, 5)] = 15,
        [(13, 20)] = 16,
        [(18, 20)] = 19,
    };

    /// <summary>
    /// \if KO
    /// <para>Combined Jong 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the combined jong value.</para>
    /// \endif
    /// </summary>
    private static readonly Dictionary<(int First, int Second), int> CombinedJong = new()
    {
        [(1, 19)] = 3,
        [(4, 22)] = 5,
        [(4, 27)] = 6,
        [(8, 1)] = 9,
        [(8, 16)] = 10,
        [(8, 17)] = 11,
        [(8, 19)] = 12,
        [(8, 25)] = 13,
        [(8, 26)] = 14,
        [(8, 27)] = 15,
        [(17, 19)] = 18,
    };

    /// <summary>
    /// \if KO
    /// <para>Split Jong 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the split jong value.</para>
    /// \endif
    /// </summary>
    private static readonly Dictionary<int, (int First, int Second)> SplitJong = CombinedJong
        .ToDictionary(x => x.Value, x => x.Key);

    /// <summary>
    /// \if KO
    /// <para>cho 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the cho value.</para>
    /// \endif
    /// </summary>
    private int _cho = -1;
    /// <summary>
    /// \if KO
    /// <para>jung 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the jung value.</para>
    /// \endif
    /// </summary>
    private int _jung = -1;
    /// <summary>
    /// \if KO
    /// <para>jong 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the jong value.</para>
    /// \endif
    /// </summary>
    private int _jong;

    /// <summary>
    /// \if KO
    /// <para>진행 중인 한글 조합 상태를 비웁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Clears the in-progress Hangul composition state.</para>
    /// \endif
    /// </summary>
    public void Reset()
    {
        _cho = -1;
        _jung = -1;
        _jong = 0;
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 문자열이 조합 가능한 단일 한글 자모인지 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Determines whether the specified string is a single composable Hangul jamo.</para>
    /// \endif
    /// </summary>
    /// <param name="text">
    /// \if KO
    /// <para>검사할 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The string to examine.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>초성 또는 중성으로 사용할 수 있는 단일 자모이면 <see langword="true"/>이고, 그렇지 않으면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> if the string is one jamo usable as an initial consonant or vowel; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// \if KO
    /// <para><paramref name="text"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="text"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public static bool IsComposableJamo(string text)
    {
        return text.Length == 1 && (ChoIndex.ContainsKey(text) || JungIndex.ContainsKey(text));
    }

    /// <summary>
    /// \if KO
    /// <para>앞선 편집기 텍스트 없이 하나의 입력을 현재 조합 상태에 적용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Applies one input to the current composition state without preceding editor text.</para>
    /// \endif
    /// </summary>
    /// <param name="text">
    /// \if KO
    /// <para>입력할 문자 또는 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The character or text to input.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>교체할 기존 문자 수와 삽입할 텍스트를 담은 편집 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An edit result containing the number of existing characters to replace and the text to insert.</para>
    /// \endif
    /// </returns>
    public HangulEdit Input(string text)
    {
        return Input(text, string.Empty);
    }

    /// <summary>
    /// \if KO
    /// <para>캐럿 앞 텍스트를 고려하여 하나의 입력을 현재 조합 상태에 적용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Applies one input to the current composition state while considering text before the caret.</para>
    /// \endif
    /// </summary>
    /// <param name="text">
    /// \if KO
    /// <para>입력할 문자 또는 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The character or text to input.</para>
    /// \endif
    /// </param>
    /// <param name="textBeforeCaret">
    /// \if KO
    /// <para>편집기에서 캐럿 앞에 있는 텍스트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The text located before the caret in the editor.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>교체할 기존 문자 수와 삽입할 텍스트를 담은 편집 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An edit result containing the number of existing characters to replace and the text to insert.</para>
    /// \endif
    /// </returns>
    public HangulEdit Input(string text, string textBeforeCaret)
    {
        if (string.IsNullOrEmpty(text) || text.Length != 1)
        {
            Reset();
            return new HangulEdit(0, text);
        }

        if (ChoIndex.TryGetValue(text, out var cho))
        {
            if (TryComposeConsonantWithTrailingText(textBeforeCaret, text, out var edit))
                return edit;

            if (!string.IsNullOrEmpty(textBeforeCaret))
            {
                Reset();
                return new HangulEdit(0, text);
            }

            return InputConsonant(text, cho);
        }

        if (JungIndex.TryGetValue(text, out var jung))
        {
            if (TryComposeVowelWithTrailingText(textBeforeCaret, jung, out var edit))
                return edit;

            if (!string.IsNullOrEmpty(textBeforeCaret))
            {
                Reset();
                return new HangulEdit(0, text);
            }

            return InputVowel(text, jung);
        }

        Reset();
        return new HangulEdit(0, text);
    }

    /// <summary>
    /// \if KO
    /// <para>캐럿 앞의 완성 음절에 입력 자음을 종성으로 결합하려고 시도합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Attempts to combine an input consonant as the final consonant of the precomposed syllable before the caret.</para>
    /// \endif
    /// </summary>
    /// <param name="textBeforeCaret">
    /// \if KO
    /// <para>캐럿 앞 텍스트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The text before the caret.</para>
    /// \endif
    /// </param>
    /// <param name="text">
    /// \if KO
    /// <para>결합할 자음입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The consonant to combine.</para>
    /// \endif
    /// </param>
    /// <param name="edit">
    /// \if KO
    /// <para>성공하면 완성된 편집 결과를 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the completed edit result when successful.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>자음을 결합했으면 <see langword="true"/>, 그렇지 않으면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> if the consonant was combined; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    private bool TryComposeConsonantWithTrailingText(string textBeforeCaret, string text, out HangulEdit edit)
    {
        edit = default;
        if (string.IsNullOrEmpty(textBeforeCaret) || !JongIndex.TryGetValue(text, out var jong))
            return false;

        var last = textBeforeCaret[^1];
        if (!TryDecompose(last, out var cho, out var jung, out var currentJong))
            return false;

        if (currentJong == 0)
        {
            _cho = cho;
            _jung = jung;
            _jong = jong;
            edit = new HangulEdit(1, Compose(_cho, _jung, _jong));
            return true;
        }

        if (CombinedJong.TryGetValue((currentJong, jong), out var combinedJong))
        {
            _cho = cho;
            _jung = jung;
            _jong = combinedJong;
            edit = new HangulEdit(1, Compose(_cho, _jung, _jong));
            return true;
        }

        return false;
    }

    /// <summary>
    /// \if KO
    /// <para>캐럿 앞 자모 또는 완성 음절과 입력 모음을 결합하려고 시도합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Attempts to combine an input vowel with a jamo or precomposed syllable before the caret.</para>
    /// \endif
    /// </summary>
    /// <param name="textBeforeCaret">
    /// \if KO
    /// <para>캐럿 앞 텍스트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The text before the caret.</para>
    /// \endif
    /// </param>
    /// <param name="jung">
    /// \if KO
    /// <para>결합할 중성 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The medial-vowel index to combine.</para>
    /// \endif
    /// </param>
    /// <param name="edit">
    /// \if KO
    /// <para>성공하면 완성된 편집 결과를 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the completed edit result when successful.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>모음을 결합했으면 <see langword="true"/>, 그렇지 않으면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> if the vowel was combined; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    private bool TryComposeVowelWithTrailingText(string textBeforeCaret, int jung, out HangulEdit edit)
    {
        edit = default;
        if (string.IsNullOrEmpty(textBeforeCaret))
            return false;

        var last = textBeforeCaret[^1].ToString();
        if (ChoIndex.TryGetValue(last, out var trailingCho))
        {
            _cho = trailingCho;
            _jung = jung;
            _jong = 0;
            edit = new HangulEdit(1, Compose(_cho, _jung, 0));
            return true;
        }

        var lastChar = textBeforeCaret[^1];
        if (!TryDecompose(lastChar, out var cho, out var currentJung, out var jong) || jong == 0)
            return false;

        if (SplitJong.TryGetValue(jong, out var split))
        {
            var previous = Compose(cho, currentJung, split.First);
            _cho = ToChoIndex(Jong[split.Second]);
            _jung = jung;
            _jong = 0;
            edit = new HangulEdit(1, previous + Compose(_cho, _jung, 0));
            return true;
        }

        _cho = ToChoIndex(Jong[jong]);
        _jung = jung;
        _jong = 0;
        edit = new HangulEdit(1, Compose(cho, currentJung, 0) + Compose(_cho, _jung, 0));
        return true;
    }

    /// <summary>
    /// \if KO
    /// <para>자음 입력을 현재 초성·중성·종성 상태에 적용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Applies a consonant input to the current initial, medial, and final consonant state.</para>
    /// \endif
    /// </summary>
    /// <param name="text">
    /// \if KO
    /// <para>입력한 자음 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input consonant string.</para>
    /// \endif
    /// </param>
    /// <param name="cho">
    /// \if KO
    /// <para>입력 자음의 초성 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The initial-consonant index of the input.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>입력 적용 후의 편집 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The edit result after applying the input.</para>
    /// \endif
    /// </returns>
    private HangulEdit InputConsonant(string text, int cho)
    {
        if (_cho < 0)
        {
            _cho = cho;
            return new HangulEdit(0, text);
        }

        if (_jung < 0)
        {
            _cho = cho;
            return new HangulEdit(0, text);
        }

        if (_jong == 0)
        {
            if (JongIndex.TryGetValue(text, out var jong))
            {
                _jong = jong;
                return new HangulEdit(1, Compose(_cho, _jung, _jong));
            }

            _cho = cho;
            _jung = -1;
            _jong = 0;
            return new HangulEdit(0, text);
        }

        if (JongIndex.TryGetValue(text, out var nextJong) &&
            CombinedJong.TryGetValue((_jong, nextJong), out var combinedJong))
        {
            _jong = combinedJong;
            return new HangulEdit(1, Compose(_cho, _jung, _jong));
        }

        _cho = cho;
        _jung = -1;
        _jong = 0;
        return new HangulEdit(0, text);
    }

    /// <summary>
    /// \if KO
    /// <para>모음 입력을 현재 초성·중성·종성 상태에 적용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Applies a vowel input to the current initial, medial, and final consonant state.</para>
    /// \endif
    /// </summary>
    /// <param name="text">
    /// \if KO
    /// <para>입력한 모음 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input vowel string.</para>
    /// \endif
    /// </param>
    /// <param name="jung">
    /// \if KO
    /// <para>입력 모음의 중성 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The medial-vowel index of the input.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>입력 적용 후의 편집 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The edit result after applying the input.</para>
    /// \endif
    /// </returns>
    private HangulEdit InputVowel(string text, int jung)
    {
        if (_cho < 0)
        {
            Reset();
            return new HangulEdit(0, text);
        }

        if (_jung < 0)
        {
            _jung = jung;
            return new HangulEdit(1, Compose(_cho, _jung, 0));
        }

        if (_jong == 0)
        {
            if (CombinedJung.TryGetValue((_jung, jung), out var combinedJung))
            {
                _jung = combinedJung;
                return new HangulEdit(1, Compose(_cho, _jung, 0));
            }

            Reset();
            return new HangulEdit(0, text);
        }

        var previousCho = _cho;
        var previousJung = _jung;
        var previousJong = _jong;

        if (SplitJong.TryGetValue(previousJong, out var split))
        {
            var previous = Compose(previousCho, previousJung, split.First);
            _cho = ToChoIndex(Jong[split.Second]);
            _jung = jung;
            _jong = 0;
            return new HangulEdit(1, previous + Compose(_cho, _jung, 0));
        }

        _cho = ToChoIndex(Jong[previousJong]);
        _jung = jung;
        _jong = 0;
        return new HangulEdit(1, Compose(previousCho, previousJung, 0) + Compose(_cho, _jung, 0));
    }

    /// <summary>
    /// \if KO
    /// <para>종성 자모에 대응하는 초성 인덱스를 찾습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Finds the initial-consonant index corresponding to a final-consonant jamo.</para>
    /// \endif
    /// </summary>
    /// <param name="jong">
    /// \if KO
    /// <para>변환할 종성 자모입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The final-consonant jamo to convert.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>대응하는 초성 인덱스이며 대응 항목이 없으면 이응 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The corresponding initial-consonant index, or the ieung index when no mapping exists.</para>
    /// \endif
    /// </returns>
    private static int ToChoIndex(string jong)
    {
        return ChoIndex.TryGetValue(jong, out var cho) ? cho : ChoIndex["ㅇ"];
    }

    /// <summary>
    /// \if KO
    /// <para>초성, 중성 및 종성 인덱스로 완성형 한글 음절을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates a precomposed Hangul syllable from initial, medial, and final consonant indexes.</para>
    /// \endif
    /// </summary>
    /// <param name="cho">
    /// \if KO
    /// <para>초성 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The initial-consonant index.</para>
    /// \endif
    /// </param>
    /// <param name="jung">
    /// \if KO
    /// <para>중성 인덱스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The medial-vowel index.</para>
    /// \endif
    /// </param>
    /// <param name="jong">
    /// \if KO
    /// <para>종성 인덱스이며 종성이 없으면 0입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The final-consonant index, or zero when no final consonant exists.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>조합된 완성형 한글 음절입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The composed precomposed Hangul syllable.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>인덱스 조합이 유효한 유니코드 코드 포인트 범위를 벗어날 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the index combination falls outside the valid Unicode code-point range.</para>
    /// \endif
    /// </exception>
    private static string Compose(int cho, int jung, int jong)
    {
        return char.ConvertFromUtf32(0xAC00 + ((cho * 21) + jung) * 28 + jong);
    }

    /// <summary>
    /// \if KO
    /// <para>완성형 한글 음절을 초성, 중성 및 종성 인덱스로 분해하려고 시도합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Attempts to decompose a precomposed Hangul syllable into initial, medial, and final consonant indexes.</para>
    /// \endif
    /// </summary>
    /// <param name="value">
    /// \if KO
    /// <para>분해할 문자입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The character to decompose.</para>
    /// \endif
    /// </param>
    /// <param name="cho">
    /// \if KO
    /// <para>성공하면 초성 인덱스를 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the initial-consonant index when successful.</para>
    /// \endif
    /// </param>
    /// <param name="jung">
    /// \if KO
    /// <para>성공하면 중성 인덱스를 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the medial-vowel index when successful.</para>
    /// \endif
    /// </param>
    /// <param name="jong">
    /// \if KO
    /// <para>성공하면 종성 인덱스를 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the final-consonant index when successful.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>문자가 완성형 한글 음절이면 <see langword="true"/>, 그렇지 않으면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> if the character is a precomposed Hangul syllable; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    private static bool TryDecompose(char value, out int cho, out int jung, out int jong)
    {
        var code = value - 0xAC00;
        if (code < 0 || code >= 11172)
        {
            cho = -1;
            jung = -1;
            jong = 0;
            return false;
        }

        cho = code / (21 * 28);
        jung = (code % (21 * 28)) / 28;
        jong = code % 28;
        return true;
    }
}

/// <summary>
/// \if KO
/// <para>한글 입력 적용 시 교체할 문자 수와 삽입할 텍스트를 나타냅니다.</para>
/// \endif
/// \if EN
/// <para>Represents the number of characters to replace and the text to insert for a Hangul input edit.</para>
/// \endif
/// </summary>
/// <param name="ReplaceCount">
/// \if KO
/// <para>캐럿 앞에서 교체할 기존 문자 수입니다.</para>
/// \endif
/// \if EN
/// <para>The number of existing characters to replace before the caret.</para>
/// \endif
/// </param>
/// <param name="Text">
/// \if KO
/// <para>교체 위치에 삽입할 텍스트입니다.</para>
/// \endif
/// \if EN
/// <para>The text to insert at the replacement position.</para>
/// \endif
/// </param>
internal readonly record struct HangulEdit(int ReplaceCount, string Text);
