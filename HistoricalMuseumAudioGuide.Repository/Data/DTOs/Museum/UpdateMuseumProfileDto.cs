namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum
{
    public class UpdateMuseumProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? OpeningHours { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? Website { get; set; }
    }
}
