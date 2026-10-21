using Microsoft.AspNetCore.Mvc;
using SlideNarrator.API.DTOs;
using SlideNarrator.BLL;
using SlideNarrator.DAL.Entities;
using SlideNarrator.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SlideNarrator.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SlidesController : ControllerBase
    {
        private readonly ITranslationService _translationService;
        private readonly ITtsService _ttsService;
        private readonly IUnitOfWork _unitOfWork;

        public SlidesController(
            ITranslationService translationService,
            ITtsService ttsService,
            IUnitOfWork unitOfWork)
        {
            _translationService = translationService;
            _ttsService = ttsService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("upload-slides")]
        public async Task<IActionResult> UploadSlides([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn ít nhất một file (PDF, JPG, PNG)." });
            }

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var service = new SlideProcessingService(webRootPath);
            var resultList = new List<ExtractedPageDto>();
            int currentPageNumber = 1;

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (ext == ".pdf")
                {
                    using var stream = file.OpenReadStream();
                    var pdfPages = service.ProcessPdf(stream, file.FileName);
                    foreach (var p in pdfPages)
                    {
                        p.PageNumber = currentPageNumber++;
                        resultList.Add(p);
                    }
                }
                else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    using var stream = file.OpenReadStream();
                    var imgPage = await service.ProcessSingleImageAsync(stream, file.FileName, currentPageNumber++);
                    resultList.Add(imgPage);
                }
                else
                {
                    return BadRequest(new { message = $"Định dạng file {ext} không được hỗ trợ. Chỉ nhận .pdf, .jpg, .png." });
                }
            }

            return Ok(new
            {
                message = "Tải file và phân trang thành công!",
                totalPages = resultList.Count,
                pages = resultList
            });
        }

        [HttpPost("process-presentation")]
        public async Task<IActionResult> ProcessPresentation([FromBody] ProcessSlideBatchRequest request)
        {
            if (request == null || request.Slides == null || request.Slides.Count == 0)
            {
                return BadRequest(new { message = "Dữ liệu slide không hợp lệ hoặc rỗng." });
            }

            // 1. Tạo dự án mới lưu vào bảng PRESENTATION_PROJECT (Dùng Guid)
            var project = new PresentationProject
            {
                Id = Guid.NewGuid(),
                Name = string.IsNullOrWhiteSpace(request.ProjectName) ? "Dự án mới" : request.ProjectName,
                SourceLanguage = request.SourceLanguage,
                SourceFileType = "pdf",
                PageCount = request.Slides.Count,
                CreatedAt = DateTime.UtcNow,
                UserAccountId = Guid.NewGuid() // Khởi tạo một Guid cho UserAccount
            };

            var responseList = new List<ProcessSlideResponseDto>();

            // 2. Lặp qua từng slide: Dịch -> Sinh Audio -> Tạo Entity
            foreach (var item in request.Slides)
            {
                // a. Dịch thuật
                var translatedText = await _translationService.TranslateAsync(
                    item.NarrativeText,
                    request.SourceLanguage,
                    request.TargetLanguage
                );

                // b. Sinh giọng nói TTS
                var audioPhysicalPath = await _ttsService.GenerateSpeechSampleAsync(translatedText, request.TargetLanguage);
                var audioFileName = Path.GetFileName(audioPhysicalPath);
                var audioWebUrl = $"/audio/{audioFileName}";

                // c. Tạo SlidePage entity (Dùng Guid và OriginalNarration)
                var slidePage = new SlidePage
                {
                    Id = Guid.NewGuid(),
                    PageNumber = item.PageNumber,
                    ImagePath = string.IsNullOrWhiteSpace(item.ImagePath) ? "/images/default_slide.png" : item.ImagePath,
                    NarrationText = item.NarrativeText,
                    PresentationProjectId = project.Id
                };

                // d. Tạo TranslationRecord entity (Dùng Guid)
                var transRecord = new TranslationRecord
                {
                    Id = Guid.NewGuid(),
                    TargetLanguage = request.TargetLanguage,
                    TranslatedText = translatedText,
                    CreatedAt = DateTime.UtcNow,
                    SlidePageId = slidePage.Id
                };

                // e. Tạo AudioTrack entity (Dùng Guid)
                var audioTrack = new AudioTrack
                {
                    Id = Guid.NewGuid(),
                    FilePath = audioPhysicalPath,
                    VoiceName = request.TargetLanguage == "vi" ? "vi-VN-Standard" : "en-US-Standard",
                    Speed = 1.0,
                    CreatedAt = DateTime.UtcNow,
                    TranslationRecordId = transRecord.Id
                };

                responseList.Add(new ProcessSlideResponseDto
                {
                    PageNumber = item.PageNumber,
                    OriginalText = item.NarrativeText,
                    TranslatedText = translatedText,
                    AudioUrl = audioWebUrl
                });
            }

            // 3. Commit toàn bộ giao dịch vào MySQL
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                message = "Xử lý dịch thuật và tạo thuyết minh thành công!",
                projectId = project.Id,
                totalSlides = responseList.Count,
                results = responseList
            });
        }
    }
}