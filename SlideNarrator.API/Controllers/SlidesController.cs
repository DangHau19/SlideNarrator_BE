using Microsoft.AspNetCore.Mvc;
using SlideNarrator.API.DTOs;
using SlideNarrator.BLL;
using SlideNarrator.BLL.DTOs;
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
        private readonly IWebHostEnvironment _env;

        public SlidesController(
            ITranslationService translationService,
            ITtsService ttsService,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment env)
        {
            _translationService = translationService;
            _ttsService = ttsService;
            _unitOfWork = unitOfWork;
            _env = env;
        }

        [HttpPost("upload-slides")]
        public async Task<IActionResult> UploadSlides([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(MakeError(400, "VALIDATION_ERROR", "Vui lòng chọn ít nhất một file (PDF, JPG, PNG)."));
            }

            var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var service = new SlideProcessingService(webRootPath);
            var resultList = new List<ExtractedPageDto>();
            var currentPageNumber = 1;

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
                    return BadRequest(MakeError(400, "UNSUPPORTED_FILE_TYPE",
                        $"Định dạng file {ext} không được hỗ trợ. Chỉ nhận .pdf, .jpg, .png."));
                }
            }

            return Ok(new
            {
                message = "Tải file và phân trang thành công!",
                totalPages = resultList.Count,
                pages = resultList
            });
        }

        /// <summary>
        /// Dịch + tạo giọng đọc cho các slide rồi LƯU vào dự án đã có (SLIDE_PAGE, TRANSLATION_RECORD, AUDIO_TRACK).
        /// Gọi lại cho cùng (dự án, số trang, ngôn ngữ) sẽ cập nhật chứ không nhân đôi dữ liệu.
        /// </summary>
        [HttpPost("process-presentation")]
        public async Task<IActionResult> ProcessPresentation([FromBody] ProcessSlideBatchRequest request)
        {
            if (request == null || request.Slides == null || request.Slides.Count == 0)
            {
                return BadRequest(MakeError(400, "VALIDATION_ERROR", "Dữ liệu slide không hợp lệ hoặc rỗng."));
            }

            if (request.ProjectId is null || request.ProjectId == Guid.Empty)
            {
                return BadRequest(MakeError(400, "VALIDATION_ERROR",
                    "Thiếu projectId. Hãy tạo dự án bằng POST /api/projects trước."));
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId.Value);
            if (project is null)
            {
                return NotFound(MakeError(404, "PROJECT_NOT_FOUND", "Không tìm thấy dự án."));
            }

            var targetLanguage = string.IsNullOrWhiteSpace(request.TargetLanguage)
                ? "vi" : request.TargetLanguage.Trim().ToLowerInvariant();
            var sourceLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage)
                ? project.SourceLanguage : request.SourceLanguage.Trim().ToLowerInvariant();

            var responseList = new List<ProcessSlideResponseDto>();

            foreach (var item in request.Slides)
            {
                if (string.IsNullOrWhiteSpace(item.NarrativeText))
                {
                    return BadRequest(MakeError(400, "VALIDATION_ERROR",
                        $"Trang {item.PageNumber} chưa có văn bản thuyết minh."));
                }

                // a + b. Dịch rồi sinh giọng đọc (không giới hạn độ dài: service tự chia đoạn).
                // Làm trước khi mở giao dịch để không giữ kết nối DB trong lúc chờ dịch vụ ngoài.
                string translatedText;
                string audioPhysicalPath;
                try
                {
                    translatedText = await _translationService.TranslateAsync(
                        item.NarrativeText, sourceLanguage, targetLanguage);
                    audioPhysicalPath = await _ttsService.GenerateSpeechSampleAsync(translatedText, targetLanguage);
                }
                catch (Exception ex)
                {
                    return StatusCode(502, MakeError(502, "EXTERNAL_SERVICE_ERROR",
                        $"Trang {item.PageNumber}: {ex.Message}"));
                }

                var audioWebUrl = $"/audio/{Path.GetFileName(audioPhysicalPath)}";
                var oldAudioUrls = new List<string>();

                // c, d, e. Lưu slide + bản dịch + audio trong MỘT giao dịch
                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var slide = await _unitOfWork.Slides.GetByPageNumberAsync(project.Id, item.PageNumber);
                    var imagePath = string.IsNullOrWhiteSpace(item.ImagePath) ? null : item.ImagePath;

                    if (slide is null)
                    {
                        slide = new SlidePage
                        {
                            PresentationProjectId = project.Id,
                            PageNumber = item.PageNumber,
                            ImagePath = imagePath ?? "/images/default_slide.png",
                            NarrationText = item.NarrativeText
                        };
                        await _unitOfWork.Slides.AddAsync(slide);
                    }
                    else
                    {
                        slide.NarrationText = item.NarrativeText;
                        if (imagePath is not null) slide.ImagePath = imagePath;
                    }
                    await _unitOfWork.SaveChangesAsync();

                    var translation = await _unitOfWork.Translations.UpsertAsync(slide.Id, targetLanguage, translatedText);
                    await _unitOfWork.SaveChangesAsync();

                    // Mỗi bản dịch chỉ giữ audio mới nhất; audio cũ bị thay thế
                    var oldTracks = await _unitOfWork.AudioTracks.FindAsync(a => a.TranslationRecordId == translation.Id);
                    foreach (var old in oldTracks)
                    {
                        oldAudioUrls.Add(old.FilePath);
                        _unitOfWork.AudioTracks.Remove(old);
                    }

                    await _unitOfWork.AudioTracks.AddAsync(new AudioTrack
                    {
                        TranslationRecordId = translation.Id,
                        FilePath = audioWebUrl,                       // lưu URL tương đối, không lưu đường dẫn ổ đĩa
                        VoiceName = $"{targetLanguage}-GoogleTranslate",
                        VoiceGender = "Female",
                        Speed = 1.0
                    });

                    project.PageCount = await _unitOfWork.Slides.CountAsync(s => s.PresentationProjectId == project.Id);
                    await _unitOfWork.SaveChangesAsync();
                });

                foreach (var url in oldAudioUrls) TryDeleteAudioFile(url);

                responseList.Add(new ProcessSlideResponseDto
                {
                    PageNumber = item.PageNumber,
                    OriginalText = item.NarrativeText,
                    TranslatedText = translatedText,
                    AudioUrl = audioWebUrl
                });
            }

            return Ok(new
            {
                message = "Xử lý dịch thuật và tạo thuyết minh thành công!",
                projectId = project.Id,
                totalSlides = responseList.Count,
                results = responseList
            });
        }

        private static void TryDeleteAudioFile(string? storedPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storedPath)) return;
                var name = Path.GetFileName(storedPath);
                if (string.IsNullOrEmpty(name)) return;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "OutputAudio", name);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            catch
            {
                // dọn file không thành công thì bỏ qua, không ảnh hưởng kết quả
            }
        }

        private static ErrorResponse MakeError(int status, string error, string message) =>
            new(status, error, message, null, DateTime.UtcNow);
    }
}
