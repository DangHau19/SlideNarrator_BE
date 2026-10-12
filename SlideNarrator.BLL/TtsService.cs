using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlideNarrator.BLL
{
    public class TtsService : ITtsService
    {
        private readonly HttpClient _httpClient;

        public TtsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateSpeechSampleAsync(string text, string language)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Nội dung thuyết minh không được để trống.");
            }

            // Chuẩn hóa ngôn ngữ mặc định nếu chưa truyền (vi: Tiếng Việt, en: Tiếng Anh)
            var langCode = string.IsNullOrWhiteSpace(language) ? "vi" : language;

            // Tạo URL gọi dịch vụ phát âm
            var encodedText = Uri.EscapeDataString(text);
            var requestUrl = $"https://translate.google.com/translate_tts?ie=UTF-8&q={encodedText}&tl={langCode}&client=tw-ob";

            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var audioBytes = await response.Content.ReadAsByteArrayAsync();

            // Tạo thư mục lưu file tạm trong ổ cứng
            var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "OutputAudio");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var fileName = $"sample_{DateTime.Now.Ticks}.mp3";
            var filePath = Path.Combine(outputDirectory, fileName);

            // Lưu file mp3 xuống máy chủ
            await File.WriteAllBytesAsync(filePath, audioBytes);

            return filePath;
        }
    }
}
