using Microsoft.AspNetCore.Http;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class UpdateMuseumMapDto
{
    public string? MapName { get; set; }
    public string? MapNameEn { get; set; }
    public string? MapType { get; set; }
    public int? FloorNumber { get; set; }
    public IFormFile? MapImage { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
}
