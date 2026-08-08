using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing
{
    public class TicketDetailDto
    {
        public int Id { get; set; }
        public string TicketCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime PurchaseDate { get; set; }
        public DateTime? ValidDate { get; set; }

        public TicketDetailTypeDto TicketType { get; set; } = null!;
        public TicketDetailMuseumDto Museum { get; set; } = null!;
        public TicketDetailExhibitionDto? Exhibition { get; set; }
        public TicketDetailOrderDto Order { get; set; } = null!;

        public string? QrCodeData { get; set; }
        public string? QrCodeImageUrl { get; set; }
    }

    public class TicketDetailTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

    public class TicketDetailMuseumDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
    }

    public class TicketDetailExhibitionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class TicketDetailOrderDto
    {
        public string OrderCode { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentStatus { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
