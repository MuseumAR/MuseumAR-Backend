namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class TicketTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int MuseumId { get; set; }
    public int? ExhibitionId { get; set; }
}
