namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    /// <summary>
    /// Slim DTO for CMS exhibit list — no translations[], no AR binary, no metadata.
    /// CMS badge columns derive from the boolean/count fields here.
    /// </summary>
    public class ExhibitListItemDto
    {
        public int Id { get; set; }
        public string? ExhibitCode { get; set; }
        public string Status { get; set; } = "Draft";
        public string? Title { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool HasArModel { get; set; }
        public int ArModelCount { get; set; }
        public bool HasAudio { get; set; }
        public bool HasQr { get; set; }
        public int? RoomId { get; set; }
        public string? RoomName { get; set; }
        public int? MapId { get; set; }
        public int? FloorNumber { get; set; }
    }
}
