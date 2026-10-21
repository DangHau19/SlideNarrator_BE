using System.Text.RegularExpressions;

namespace SlideNarrator.BLL;

/// <summary>
/// Chia văn bản dài thành nhiều đoạn ngắn (ưu tiên cắt ở cuối câu, rồi dấu phẩy, rồi khoảng trắng)
/// để gửi cho các dịch vụ dịch / TTS vốn chỉ nhận đoạn ngắn. Nhờ vậy người dùng không bị giới hạn số ký tự.
/// </summary>
public static class TextChunker
{
    private const string SentenceEnds = ".!?;。！？";
    private const string SoftBreaks = ",:，、";

    public static List<string> Split(string? text, int maxLength)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        if (maxLength < 20) maxLength = 20;

        // Gộp mọi khoảng trắng/xuống dòng thành một dấu cách để cắt đoạn ổn định
        text = Regex.Replace(text.Trim(), @"\s+", " ");

        var start = 0;
        while (start < text.Length)
        {
            if (text.Length - start <= maxLength)
            {
                result.Add(text.Substring(start).Trim());
                break;
            }

            var slice = text.Substring(start, maxLength);
            var cut = FindBreak(slice);
            var piece = text.Substring(start, cut).Trim();
            if (piece.Length > 0) result.Add(piece);

            start += cut;
            while (start < text.Length && text[start] == ' ') start++;
        }

        return result;
    }

    private static int FindBreak(string slice)
    {
        var min = slice.Length / 3;

        // 1. Cuối câu
        for (var i = slice.Length - 1; i >= min; i--)
        {
            if (SentenceEnds.IndexOf(slice[i]) >= 0 && (i == slice.Length - 1 || slice[i + 1] == ' '))
                return i + 1;
        }

        // 2. Dấu phẩy, hai chấm
        for (var i = slice.Length - 1; i >= min; i--)
        {
            if (SoftBreaks.IndexOf(slice[i]) >= 0 && (i == slice.Length - 1 || slice[i + 1] == ' '))
                return i + 1;
        }

        // 3. Khoảng trắng gần cuối nhất (cắt trước dấu cách)
        var space = slice.LastIndexOf(' ');
        if (space >= min) return space;

        // 4. Không có chỗ nào để cắt (chuỗi liền không dấu cách): cắt cứng
        return slice.Length;
    }
}

/// <summary>Chuẩn hóa mã ngôn ngữ của hệ thống sang mã mà Google Translate / TTS hiểu.</summary>
public static class GoogleLanguage
{
    public static string Normalize(string? code)
    {
        var c = string.IsNullOrWhiteSpace(code) ? "vi" : code.Trim().ToLowerInvariant();
        return c switch
        {
            "zh" => "zh-CN",
            "jp" => "ja",
            _ => c
        };
    }
}
