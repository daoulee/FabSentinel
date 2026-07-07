namespace GreenVision.Services;

public interface ILocalizationService
{
    string CurrentLanguage { get; }
    string Get(string key);
    void SetLanguage(string language);
    event EventHandler? LanguageChanged;

    // Shorthand
    string this[string key] => Get(key);
}
