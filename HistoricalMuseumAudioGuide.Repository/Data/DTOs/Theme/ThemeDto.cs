namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme
{
    public class ThemeDto
    {
        public int Id { get; set; }
        public int? MuseumId { get; set; }
        public string ThemeName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
