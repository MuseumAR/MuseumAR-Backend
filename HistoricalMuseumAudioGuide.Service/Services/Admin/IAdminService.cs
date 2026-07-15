using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.User;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin
{
    public interface IAdminService
    {
        // Museum Profile Management
        Task<ResponseModel> GetMuseumProfileAsync();
        Task<ResponseModel> UpdateMuseumProfileAsync(UpdateMuseumProfileDto museumDto);

        // Ticket Type Management
        Task<ResponseModel> GetAllTicketTypesAsync();
        Task<ResponseModel> CreateTicketTypeAsync(CreateTicketTypeDto ticketTypeDto);
        Task<ResponseModel> ApproveTicketTypeAsync(int id);
        Task<ResponseModel> RejectTicketTypeAsync(int id);

        // User Management
        Task<ResponseModel> GetAllUsersAsync(string? roleName, string? status, string? search);
        Task<ResponseModel> GetUserByIdAsync(int id);
        Task<ResponseModel> CreateUserAsync(CreateUserDto dto);
        Task<ResponseModel> UpdateUserAsync(int id, UpdateUserDto dto);
        Task<ResponseModel> DeleteUserAsync(int id);

        // Audit Logs
        Task<ResponseModel> GetAuditLogsAsync(int? userId, string? action, System.DateTime? fromDate, System.DateTime? toDate, int page, int pageSize);
    }
}
