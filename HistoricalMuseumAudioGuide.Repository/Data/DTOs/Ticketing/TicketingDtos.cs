using System;

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

public class CreateOrderRequestDto
{
    public int TicketTypeId { get; set; }
    public int Quantity { get; set; }
}

public class TicketDto
{
    public int Id { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketTypeName { get; set; } = null!;
    public DateTime PurchaseDate { get; set; }
    public DateTime? ValidDate { get; set; }
    public string Status { get; set; } = null!;
}
