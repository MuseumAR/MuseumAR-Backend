using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition
{
    public class CreateExhibitionDto
    {
        public int MuseumId { get; set; }
        public int? ThemeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
