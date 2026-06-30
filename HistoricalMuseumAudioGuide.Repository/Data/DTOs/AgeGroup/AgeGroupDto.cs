namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.AgeGroup
{
    public class AgeGroupDto
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = null!;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }
}
