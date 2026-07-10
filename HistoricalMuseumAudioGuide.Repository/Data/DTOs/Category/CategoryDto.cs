using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public int? MuseumId { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? IconUrl { get; set; }
        public string Status { get; set; } = null!;
        public ICollection<CategoryTranslationDto> CategoryTranslations { get; set; } = new List<CategoryTranslationDto>();
    }
}
