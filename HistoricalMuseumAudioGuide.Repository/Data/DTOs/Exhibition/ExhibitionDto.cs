using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition
{
    public class ExhibitionDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public int? ThemeId { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public List<ExhibitionTranslationDto> Translations { get; set; } = new();
    }
}
