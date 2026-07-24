using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing
{
    public class ApprovalTicketTypeDto
    {
        [Required]
        [RegularExpression("Approved|Rejected", ErrorMessage = "Trạng thái chỉ nhận 'Approved' hoặc 'Rejected'")]
        public string Status { get; set; } = "Approved";
    }
}