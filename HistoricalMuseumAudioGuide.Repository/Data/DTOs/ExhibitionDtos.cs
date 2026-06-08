using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs
{
    public class ExhibitionDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class CreateExhibitionDto
    {
        public int MuseumId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
