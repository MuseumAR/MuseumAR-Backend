using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Ticketing;

public interface ITicketingService
{
    Task<ResponseModel> GetTicketTypesAsync();
    Task<ResponseModel> CreateOrderAsync(int visitorId, CreateOrderRequestDto request);
    Task<ResponseModel> HandleVnPayIpnAsync(IDictionary<string, string> queryParams);
    Task<ResponseModel> ConfirmMockPaymentAsync(string orderCode);
    Task<ResponseModel> GetMyTicketsAsync(int visitorId);
}
