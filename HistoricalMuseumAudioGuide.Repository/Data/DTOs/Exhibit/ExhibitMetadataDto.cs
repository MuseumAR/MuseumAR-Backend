namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    public class ExhibitMetadataDto
    {
        public int? ThemeId { get; set; }
        public int? AgeGroupId { get; set; }
        public string? Era { get; set; }
        public string? HistoricalEvent { get; set; }
    }
}
