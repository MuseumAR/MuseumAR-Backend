using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class TicketDto
{
    public int Id { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketTypeName { get; set; } = null!;
    public DateTime PurchaseDate { get; set; }
    public DateTime? ValidDate { get; set; }
    public string Status { get; set; } = null!;
}
