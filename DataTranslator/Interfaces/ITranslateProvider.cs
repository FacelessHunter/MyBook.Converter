namespace DataTranslator.Interfaces
{
    public interface ITranslateProvider : IDisposable
    {
        Task InitializeProviderAsync();


        Task<string> TranslateAsync(string text, string fromLang = "en", string toLang = "uk", string separationSequence = null);
    }
}
