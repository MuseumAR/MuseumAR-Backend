using System;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TicketPromotion
{
    public int Id { get; set; }

    public int TicketTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    /// <summary>
    /// "Percentage" hoặc "FixedAmount"
    /// </summary>
    public string DiscountType { get; set; } = "Percentage";

    /// <summary>
    /// Giá trị giảm: ví dụ 20 (20%) hoặc 10000 (giảm 10,000 VND)
    /// </summary>
    public decimal DiscountValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TicketType TicketType { get; set; } = null!;
}
