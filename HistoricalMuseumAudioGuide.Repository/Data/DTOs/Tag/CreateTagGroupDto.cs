namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag
{
    public class CreateTagGroupDto
    {
        public string GroupName { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
