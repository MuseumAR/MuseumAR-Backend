using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Text;

namespace HistoricalMuseumAudioGuide.Service.Services.Payment
{
    public interface IPaymentService
    {
        Task<ResponseModel> CreatePaymentLinkAsync(string orderCode);
        Task<ResponseModel> ProcessPayOSWebhookAsync(Webhook webhookBody);
    }
}
