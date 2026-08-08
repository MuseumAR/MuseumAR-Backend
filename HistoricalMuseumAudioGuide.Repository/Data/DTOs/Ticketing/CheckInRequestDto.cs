using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class CheckInRequestDto
{
    [Required]
    public string TicketCode { get; set; } = null!;
}
