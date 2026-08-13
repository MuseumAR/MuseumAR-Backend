using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag
{
    public class CreateTagDto
    {
        public int TagGroupId { get; set; }
        public string TagName { get; set; } = null!;
        public int SortOrder { get; set; }
        public ICollection<TagTranslationDto>? Translations { get; set; }
    }
}
