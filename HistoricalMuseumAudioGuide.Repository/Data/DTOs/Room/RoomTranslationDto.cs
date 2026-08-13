namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room;

public class RoomTranslationDto
{
    public string LanguageCode { get; set; } = null!;
    public string RoomName { get; set; } = null!;
    public string? Description { get; set; }
}
