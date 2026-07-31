using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Ticketing;

public interface ITicketingService
{
    Task<ResponseModel> GetTicketTypesAsync(string? lang = null);
    Task<ResponseModel> CreateOrderAsync(int visitorId, CreateOrderRequestDto request);
    Task<ResponseModel> GetMyTicketsAsync(int visitorId);
    Task<ResponseModel> MockConfirmPaymentAsync(string orderCode);
}
