namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag
{
    public class TagDto
    {
        public int Id { get; set; }
        public int TagGroupId { get; set; }
        public string TagName { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
