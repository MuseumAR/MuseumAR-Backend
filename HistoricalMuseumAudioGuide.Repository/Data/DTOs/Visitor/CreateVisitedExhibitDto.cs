namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;

public class CreateVisitedExhibitDto
{
    public int ExhibitId { get; set; }
    public decimal? TimeSpentSeconds { get; set; }
}
