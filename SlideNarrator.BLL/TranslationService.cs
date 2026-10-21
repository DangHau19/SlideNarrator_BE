using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlideNarrator.BLL
{
    public class TranslationService : ITranslationService
    {
        // Gửi bằng GET nên mỗi lần chỉ dịch một đoạn ngắn để URL không vượt giới hạn độ dài.
        private const int MaxChunkLength = 450;
        private const int MaxAttempts = 3;

        private readonly HttpClient _httpClient;

        public TranslationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Thêm User-Agent để Google không chặn request
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }
        }

        public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            if (string.Equals(sourceLanguage?.Trim(), targetLanguage?.Trim(), StringComparison.OrdinalIgnoreCase))
                return text;

            var sl = GoogleLanguage.Normalize(sourceLanguage);
            var tl = GoogleLanguage.Normalize(targetLanguage);

            var result = new StringBuilder();
            foreach (var chunk in TextChunker.Split(text, MaxChunkLength))
            {
                var translated = await TranslateChunkAsync(chunk, sl, tl);
                if (result.Length > 0) result.Append(' ');
                result.Append(translated.Trim());
            }

            return result.ToString();
        }

        private async Task<string> TranslateChunkAsync(string chunk, string sl, string tl)
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sl}&tl={tl}&dt=t&q={Uri.EscapeDataString(chunk)}";

            Exception? last = null;
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(response);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0 &&
                        root[0].ValueKind == JsonValueKind.Array)
                    {
                        var sb = new StringBuilder();
                        foreach (var sentence in root[0].EnumerateArray())
                        {
                            if (sentence.ValueKind == JsonValueKind.Array && sentence.GetArrayLength() > 0 &&
                                sentence[0].ValueKind == JsonValueKind.String)
                            {
                                sb.Append(sentence[0].GetString());
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(sb.ToString())) return sb.ToString();
                    }

                    last = new InvalidOperationException("Dịch vụ dịch thuật trả về kết quả rỗng.");
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    last = ex;
                }

                await Task.Delay(500 * attempt);
            }

            // Không còn lặng lẽ trả lại văn bản gốc: báo lỗi để giao diện hiển thị đúng.
            throw new InvalidOperationException($"Không dịch được văn bản: {last?.Message}");
        }
    }
}
