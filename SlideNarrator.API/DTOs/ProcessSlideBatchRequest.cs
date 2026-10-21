using System.Collections.Generic;

namespace SlideNarrator.API.DTOs
{
    public class SlideItemDto
    {
        public int PageNumber { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string NarrativeText { get; set; } = string.Empty;
    }

    public class ProcessSlideBatchRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = "en";
        public string TargetLanguage { get; set; } = "vi";
        public List<SlideItemDto> Slides { get; set; } = new();
    }

    public class ProcessSlideResponseDto
    {
        public int PageNumber { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
    }
}