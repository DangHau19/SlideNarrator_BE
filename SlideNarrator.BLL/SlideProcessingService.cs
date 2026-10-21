using System;
using System.Collections.Generic;
using System.IO;
using UglyToad.PdfPig;

namespace SlideNarrator.BLL
{
    public class ExtractedPageDto
    {
        public int PageNumber { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
    }

    public class SlideProcessingService
    {
        private readonly string _uploadDir;

        public SlideProcessingService(string webRootPath)
        {
            _uploadDir = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(_uploadDir))
            {
                Directory.CreateDirectory(_uploadDir);
            }
        }

        // 1. Xử lý file PDF: Bóc tách text từng trang và gán ảnh đại diện
        public List<ExtractedPageDto> ProcessPdf(Stream pdfStream, string fileName)
        {
            var pages = new List<ExtractedPageDto>();
            using (var document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    var text = string.Join(" ", page.GetWords());
                    pages.Add(new ExtractedPageDto
                    {
                        PageNumber = page.Number,
                        ImageUrl = "/images/pdf_icon.png", // icon hoặc ảnh thumbnail
                        ExtractedText = string.IsNullOrWhiteSpace(text) ? "" : text.Trim()
                    });
                }
            }
            return pages;
        }

        // 2. Xử lý file ảnh đơn (JPG / PNG): Lưu file vào wwwroot/uploads và trả về đường dẫn URL
        public async System.Threading.Tasks.Task<ExtractedPageDto> ProcessSingleImageAsync(Stream imageStream, string originalFileName, int pageIndex)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var uniqueFileName = $"slide_{Guid.NewGuid()}{extension}";
            var physicalPath = Path.Combine(_uploadDir, uniqueFileName);

            using (var fileStream = new FileStream(physicalPath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            return new ExtractedPageDto
            {
                PageNumber = pageIndex,
                ImageUrl = $"/uploads/{uniqueFileName}",
                ExtractedText = "" // Với ảnh thì để trống để người dùng nhập/dán lời thuyết minh
            };
        }
    }
}
