namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class CreateMuseumMapDto
{
    public int MuseumId { get; set; }
    public string MapImageUrl { get; set; } = null!;
    public string MapType { get; set; } = null!;
}
