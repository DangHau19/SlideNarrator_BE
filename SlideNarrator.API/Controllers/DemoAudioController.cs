using Microsoft.AspNetCore.Mvc;
using SlideNarrator.API.DTOs;
using SlideNarrator.BLL;
using System.Threading.Tasks;

namespace SlideNarrator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoAudioController : ControllerBase
    {
        private readonly ITtsService _ttsService;

        public DemoAudioController(ITtsService ttsService)
        {
            _ttsService = ttsService;
        }

        [HttpPost("generate-speech")]
        public async Task<IActionResult> GenerateAudio([FromBody] CreateAudioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TextContent))
            {
                return BadRequest(new { message = "Nội dung thuyết minh không được để trống." });
            }

            try
            {
                var filePath = await _ttsService.GenerateSpeechSampleAsync(request.TextContent, request.TargetLanguage);
                return Ok(new
                {
                    message = "Tạo audio thuyết minh cho bài giảng thành công!",
                    project = request.ProjectName,
                    text = request.TextContent,
                    language = request.TargetLanguage,
                    audioUrl = filePath
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}