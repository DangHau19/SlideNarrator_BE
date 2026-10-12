using System;
using System.Collections.Generic;
using System.Text;

namespace SlideNarrator.BLL
{
    public interface ITtsService
    {
        Task<string> GenerateSpeechSampleAsync(string text, string language);
    }
}
