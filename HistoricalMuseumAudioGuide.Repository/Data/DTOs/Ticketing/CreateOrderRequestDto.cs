namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class CreateOrderRequestDto
{
    public int TicketTypeId { get; set; }
    public int Quantity { get; set; }
}
