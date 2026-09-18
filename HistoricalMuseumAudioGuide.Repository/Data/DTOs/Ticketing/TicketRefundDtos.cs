using System;
using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

public class CreateTicketRefundRequestDto
{
    [Required(ErrorMessage = "Tên ngân hàng không được để trống")]
    [StringLength(100)]
    public string BankName { get; set; } = null!;

    [Required(ErrorMessage = "Số tài khoản không được để trống")]
    [StringLength(50)]
    public string AccountNumber { get; set; } = null!;

    [Required(ErrorMessage = "Tên chủ tài khoản không được để trống")]
    [StringLength(100)]
    public string AccountHolderName { get; set; } = null!;

    [Required(ErrorMessage = "Lý do hoàn vé không được để trống")]
    [StringLength(500)]
    public string Reason { get; set; } = null!;
}

public class ProcessTicketRefundRequestDto
{
    public bool IsApproved { get; set; }

    [StringLength(500)]
    public string? RejectReason { get; set; }
}

public class TicketRefundRequestDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketTypeName { get; set; } = null!;
    public int VisitorId { get; set; }
    public string VisitorName { get; set; } = null!;
    public string? VisitorEmail { get; set; }
    public string? VisitorPhone { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string AccountHolderName { get; set; } = null!;
    public string Status { get; set; } = null!; // "Pending", "Approved", "Rejected"
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
