namespace SlideNarrator.API.DTOs
{
    public class CreateAudioRequest
    {
        public string ProjectName { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = "en";
        public string TargetLanguage { get; set; } = "vi";
    }
}