using System.Text;

namespace PsychometricTestDesigner.Frontend.Services;

public static class RuText
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly string[] MojibakeMarkers =
    [
        "Рђ", "Р‘", "Р’", "Р“", "Р”", "Р•", "Р–", "Р—", "Р\u0098", "Р™", "Рљ", "Р›",
        "Рњ", "Рќ", "Рџ", "РЎ", "Рў", "РЈ", "Р¤", "РҐ", "Р¦", "Р§", "РЁ", "Р©",
        "Р°", "Р±", "Р²", "Рі", "Рґ", "Рµ", "Рё", "Р№", "Рє", "Р»", "Рј", "РЅ",
        "Рѕ", "Рї", "СЃ", "С‚", "СЊ", "С‹", "СЏ", "СЂ", "С‡", "С€", "С‰", "С‘"
    ];

    private static readonly Dictionary<char, byte> Cp1251 = new()
    {
        ['Ђ'] = 0x80, ['Ѓ'] = 0x81, ['‚'] = 0x82, ['ѓ'] = 0x83, ['„'] = 0x84, ['…'] = 0x85, ['†'] = 0x86, ['‡'] = 0x87,
        ['€'] = 0x88, ['‰'] = 0x89, ['Љ'] = 0x8A, ['‹'] = 0x8B, ['Њ'] = 0x8C, ['Ќ'] = 0x8D, ['Ћ'] = 0x8E, ['Џ'] = 0x8F,
        ['ђ'] = 0x90, ['‘'] = 0x91, ['’'] = 0x92, ['“'] = 0x93, ['”'] = 0x94, ['•'] = 0x95, ['–'] = 0x96, ['—'] = 0x97,
        ['™'] = 0x99, ['љ'] = 0x9A, ['›'] = 0x9B, ['њ'] = 0x9C, ['ќ'] = 0x9D, ['ћ'] = 0x9E, ['џ'] = 0x9F,
        ['\u00A0'] = 0xA0, ['Ў'] = 0xA1, ['ў'] = 0xA2, ['Ј'] = 0xA3, ['¤'] = 0xA4, ['Ґ'] = 0xA5, ['¦'] = 0xA6, ['§'] = 0xA7,
        ['Ё'] = 0xA8, ['©'] = 0xA9, ['Є'] = 0xAA, ['«'] = 0xAB, ['¬'] = 0xAC, ['\u00AD'] = 0xAD, ['®'] = 0xAE, ['Ї'] = 0xAF,
        ['°'] = 0xB0, ['±'] = 0xB1, ['І'] = 0xB2, ['і'] = 0xB3, ['ґ'] = 0xB4, ['µ'] = 0xB5, ['¶'] = 0xB6, ['·'] = 0xB7,
        ['ё'] = 0xB8, ['№'] = 0xB9, ['є'] = 0xBA, ['»'] = 0xBB, ['ј'] = 0xBC, ['Ѕ'] = 0xBD, ['ѕ'] = 0xBE, ['ї'] = 0xBF
    };

    public static string Fix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = value.Trim();
        if (!MojibakeMarkers.Any(text.Contains))
        {
            return text;
        }

        var bytes = new List<byte>(text.Length);
        foreach (var ch in text)
        {
            if (TryMapCp1251(ch, out var b))
            {
                bytes.Add(b);
                continue;
            }

            return text;
        }

        try
        {
            var decoded = StrictUtf8.GetString(bytes.ToArray());
            return decoded.Contains('\uFFFD') ? text : decoded;
        }
        catch (DecoderFallbackException)
        {
            return text;
        }
    }

    public static string RiskLabel(string? value) => value?.ToLowerInvariant() switch
    {
        "critical" => "Критический",
        "high" => "Высокий",
        "medium" => "Средний",
        "low" => "Низкий",
        _ => "Нет данных"
    };

    public static string SeverityLabel(string? value) => value?.ToLowerInvariant() switch
    {
        "critical" => "Критично",
        "high" => "Высокий",
        "warning" => "Высокий",
        "medium" => "Средний",
        "low" => "Низкий",
        _ => "Информация"
    };

    public static string Percent(decimal value) => $"{Math.Round(value, 1)}%";

    private static bool TryMapCp1251(char ch, out byte value)
    {
        if (ch <= 0x7F)
        {
            value = (byte)ch;
            return true;
        }

        if (ch is >= (char)0x80 and <= (char)0x9F)
        {
            value = (byte)ch;
            return true;
        }

        if (ch is >= 'А' and <= 'я')
        {
            value = (byte)(0xC0 + ch - 'А');
            return true;
        }

        return Cp1251.TryGetValue(ch, out value);
    }
}

