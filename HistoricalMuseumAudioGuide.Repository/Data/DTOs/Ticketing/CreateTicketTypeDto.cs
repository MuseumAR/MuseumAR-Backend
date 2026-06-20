namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class CreateTicketTypeDto
{
    public int MuseumId { get; set; }
    public int? ExhibitionId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
