using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SlideNarrator.BLL
{
    public class TtsService : ITtsService
    {
        // Endpoint TTS chỉ đọc ổn định ~200 ký tự mỗi lần gọi, nên văn bản dài được chia nhỏ rồi ghép file mp3 lại.
        private const int MaxChunkLength = 180;
        private const int MaxAttempts = 3;

        private readonly HttpClient _httpClient;

        public TtsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }
        }

        public async Task<string> GenerateSpeechSampleAsync(string text, string language)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Nội dung thuyết minh không được để trống.");
            }

            var langCode = GoogleLanguage.Normalize(language);
            var chunks = TextChunker.Split(text, MaxChunkLength);

            // Các frame MP3 nối liền nhau vẫn phát được, nên chỉ cần ghép byte của từng đoạn.
            using var output = new MemoryStream();
            for (var i = 0; i < chunks.Count; i++)
            {
                var bytes = await DownloadChunkAsync(chunks[i], langCode, i, chunks.Count);
                output.Write(bytes, 0, bytes.Length);

                if (i < chunks.Count - 1) await Task.Delay(150); // tránh gọi dồn dập bị chặn
            }

            var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "OutputAudio");
            Directory.CreateDirectory(outputDirectory);

            var fileName = $"tts_{Guid.NewGuid():N}.mp3";
            var filePath = Path.Combine(outputDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, output.ToArray());

            return filePath;
        }

        private async Task<byte[]> DownloadChunkAsync(string chunk, string langCode, int index, int total)
        {
            var url = "https://translate.google.com/translate_tts?ie=UTF-8" +
                      $"&q={Uri.EscapeDataString(chunk)}&tl={langCode}" +
                      $"&total={total}&idx={index}&textlen={chunk.Length}&client=tw-ob";

            Exception? last = null;
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes.Length > 0) return bytes;
                    last = new InvalidOperationException("Dịch vụ giọng đọc trả về dữ liệu rỗng.");
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    last = ex;
                }

                await Task.Delay(500 * attempt);
            }

            throw new InvalidOperationException(
                $"Không tạo được giọng đọc (đoạn {index + 1}/{total}): {last?.Message}");
        }
    }
}
