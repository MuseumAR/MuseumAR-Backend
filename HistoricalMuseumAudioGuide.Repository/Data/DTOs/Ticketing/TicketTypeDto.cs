namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class TicketTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public int MuseumId { get; set; }
    public int? ExhibitionId { get; set; }
    public string Status { get; set; } = null!;
    public bool IsActive { get; set; }

    // Promotion fields
    public System.Collections.Generic.List<TicketPromotionDto>? ActivePromotions { get; set; }
    public decimal? OriginalPrice { get; set; }
}
