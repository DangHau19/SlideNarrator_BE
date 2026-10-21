using System.Threading.Tasks;

namespace SlideNarrator.BLL
{
    public interface ITranslationService
    {
        Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage);
    }
}