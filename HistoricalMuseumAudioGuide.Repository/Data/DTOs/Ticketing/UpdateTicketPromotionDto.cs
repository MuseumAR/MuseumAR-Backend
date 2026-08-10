using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class UpdateTicketPromotionDto
{
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// "Percentage" hoặc "FixedAmount"
    /// </summary>
    public string DiscountType { get; set; } = "Percentage";

    /// <summary>
    /// Ví dụ: 20 (giảm 20%) hoặc 10000 (giảm 10,000 VND)
    /// </summary>
    public decimal DiscountValue { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
