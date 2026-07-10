namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme
{
    public class CreateThemeDto
    {
        public int? MuseumId { get; set; }
        public string ThemeName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
