namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;

public class OfflinePackageTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string? PackageName { get; set; }
    public string? Description { get; set; }
}
