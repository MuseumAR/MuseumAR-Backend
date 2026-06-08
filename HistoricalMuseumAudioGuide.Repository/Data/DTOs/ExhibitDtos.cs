using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs
{
    public class ExhibitDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public string? ExhibitCode { get; set; }
        public string Status { get; set; } = "Draft";
        public string? ThumbnailUrl { get; set; }
    }

    public class CreateExhibitDto
    {
        public int MuseumId { get; set; }
        public int? CategoryId { get; set; }
        public string? ExhibitCode { get; set; }
        public string Status { get; set; } = "Draft";
    }
}
