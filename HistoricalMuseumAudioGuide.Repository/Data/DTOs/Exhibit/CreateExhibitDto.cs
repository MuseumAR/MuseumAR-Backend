using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    public class CreateExhibitDto
    {
        public int MuseumId { get; set; }
        public int? CategoryId { get; set; }
        public string? ExhibitCode { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AROverlayUrl { get; set; }
        public string? ARMarkerUrl { get; set; }
        public string Status { get; set; } = "Draft";
        
        // Include initial translation (e.g., in default language)
        public ICollection<ExhibitTranslationDto> Translations { get; set; } = new List<ExhibitTranslationDto>();
    }
}
