using System;
using System.Collections.Generic;
using System.Text;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class CreateBookingDto
{
    public int PaymentMethodId { get; set; }
    public DateTime ValidDate { get; set; }
    public List<BookingItemDto> Items { get; set; } = new();
}

public class BookingItemDto
{
    public int TicketTypeId { get; set; }
    public int Quantity { get; set; } // Số lượng vé đặt mua (>= 1)
}