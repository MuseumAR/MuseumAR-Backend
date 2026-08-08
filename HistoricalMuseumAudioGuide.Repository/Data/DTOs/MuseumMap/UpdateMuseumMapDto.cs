using Microsoft.AspNetCore.Http;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class UpdateMuseumMapDto
{
    public string? MapType { get; set; }
    public int? FloorNumber { get; set; }
    public IFormFile? MapImage { get; set; }
}
