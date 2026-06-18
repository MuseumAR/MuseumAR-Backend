namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class MuseumMapDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public string MapImageUrl { get; set; } = null!;
    public string MapType { get; set; } = null!;
}
