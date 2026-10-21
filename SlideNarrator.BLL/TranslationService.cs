using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SlideNarrator.BLL
{
    public class TranslationService : ITranslationService
    {
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
            if (sourceLanguage.Equals(targetLanguage, StringComparison.OrdinalIgnoreCase)) return text;

            try
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={Uri.EscapeDataString(text)}";
                var response = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var sentences = root[0];
                    var fullTranslatedText = "";
                    foreach (var sentence in sentences.EnumerateArray())
                    {
                        if (sentence.GetArrayLength() > 0)
                        {
                            fullTranslatedText += sentence[0].GetString();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fullTranslatedText))
                    {
                        return fullTranslatedText;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi Dịch Thuật]: {ex.Message}");
            }

            return text; // Nếu lỗi mạng thì giữ nguyên
        }
    }
}