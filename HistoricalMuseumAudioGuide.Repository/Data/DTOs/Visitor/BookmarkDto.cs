using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;

public class BookmarkDto
{
    public int Id { get; set; }
    public int VisitorId { get; set; }
    public int ExhibitId { get; set; }
    public DateTime CreatedAt { get; set; }
}
