using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin
{
    public interface IAdminService
    {
        // Museum Management
        Task<ResponseModel> GetAllMuseumsAsync();
        Task<ResponseModel> GetMuseumByIdAsync(int id);
        Task<ResponseModel> CreateMuseumAsync(CreateMuseumDto museumDto);
        Task<ResponseModel> UpdateMuseumAsync(int id, MuseumDto museumDto);
        Task<ResponseModel> DeleteMuseumAsync(int id);

        // Ticket Type Management
        Task<ResponseModel> GetAllTicketTypesAsync();
        Task<ResponseModel> CreateTicketTypeAsync(CreateTicketTypeDto ticketTypeDto);
    }
}
