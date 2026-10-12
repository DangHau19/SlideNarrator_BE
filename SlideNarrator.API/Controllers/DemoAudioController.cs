using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateAudio([FromQuery] string text, [FromQuery] string language = "vi")
        {
            try
            {
                var filePath = await _ttsService.GenerateSpeechSampleAsync(text, language);
                return Ok(new
                {
                    message = "Tạo audio thuyết minh thành công!",
                    savedPath = filePath,
                    sampleText = text,
                    lang = language
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}