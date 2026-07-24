using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class CreateTicketTypeDto
{
    [Required]
    public int MuseumId { get; set; }

    public int? ExhibitionId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Giá vé không được nhỏ hơn 0.")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}