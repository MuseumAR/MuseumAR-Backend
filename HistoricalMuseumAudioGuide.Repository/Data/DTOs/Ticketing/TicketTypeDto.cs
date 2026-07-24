namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class TicketTypeDto
{
    public int Id { get; set; }
    public int MuseumId { get; set; }
    public int? ExhibitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
