namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;

public class MapPoiDto
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public string PoiType { get; set; } = null!;
    public double LocationX { get; set; }
    public double LocationY { get; set; }
    public string? Description { get; set; }
}

public class CreateMapPoiDto
{
    public int MapId { get; set; }
    public string PoiType { get; set; } = "WC";
    public double LocationX { get; set; }
    public double LocationY { get; set; }
    public string? Description { get; set; }
}

public class UpdateMapPoiDto
{
    public string? PoiType { get; set; }
    public double? LocationX { get; set; }
    public double? LocationY { get; set; }
    public string? Description { get; set; }
}
