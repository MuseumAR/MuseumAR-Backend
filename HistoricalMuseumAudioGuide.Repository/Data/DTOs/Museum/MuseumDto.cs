namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum
{
    public class MuseumDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string Status { get; set; } = "Active";
        public string? ThumbnailUrl { get; set; }
    }
}
