namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag
{
    public class TagGroupDto
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
