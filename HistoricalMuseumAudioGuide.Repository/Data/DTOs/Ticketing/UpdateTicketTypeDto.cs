namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class UpdateTicketTypeDto
{
    public int? ExhibitionId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public bool IsActive { get; set; }
}
