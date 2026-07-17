using Microsoft.AspNetCore.Http;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class CreateMuseumMapDto
{
    public int MuseumId { get; set; }
    public IFormFile MapImage { get; set; } = null!;
    public string MapType { get; set; } = null!;
    public string MapName { get; set; } = null!;
    public int FloorNumber { get; set; }
}
