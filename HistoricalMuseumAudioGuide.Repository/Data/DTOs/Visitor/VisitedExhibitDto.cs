using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;

public class VisitedExhibitDto
{
    public int Id { get; set; }
    public int VisitorId { get; set; }
    public int ExhibitId { get; set; }
    public DateTime VisitedAt { get; set; }
    public decimal? TimeSpentSeconds { get; set; }
}
