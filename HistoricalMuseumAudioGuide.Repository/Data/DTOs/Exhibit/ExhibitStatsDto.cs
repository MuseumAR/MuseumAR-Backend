namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    public class ExhibitStatsDto
    {
        public int Total { get; set; }
        public int Published { get; set; }
        public int Draft { get; set; }
        public int WithArModel { get; set; }
        public int WithAr => WithArModel; // Tương thích ngược với frontend cũ
        public int WithQr { get; set; }
    }
}
