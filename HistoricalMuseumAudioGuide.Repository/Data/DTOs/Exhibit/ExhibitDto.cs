using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    public class ExhibitDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public int? CategoryId { get; set; }
        public string? ExhibitCode { get; set; }
        public string? QRCodeData { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AROverlayUrl { get; set; }
        public string? ARMarkerUrl { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? PublishedAt { get; set; }
        
        public ExhibitMetadataDto? ExhibitMetadata { get; set; }
        
        public ICollection<ExhibitTranslationDto> Translations { get; set; } = new List<ExhibitTranslationDto>();
    }
}
