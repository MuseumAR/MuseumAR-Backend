namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    public class ExhibitTranslationDto
    {
        public int? Id { get; set; }
        public int ExhibitId { get; set; }
        public string LanguageCode { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }
        public int? AudioDuration { get; set; }
    }
}
