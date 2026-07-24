using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using System;
using System.Collections.Generic;
using System.Text;

namespace HistoricalMuseumAudioGuide.Service.Services.Ticketing;

public interface ITicketTypeService
{
    Task<ResponseModel> GetTicketTypesAsync(int? museumId, bool activeOnly = true);
    Task<ResponseModel> GetAllForCMSAsync(int? museumId = null);
    Task<ResponseModel> GetPendingTicketTypesAsync();
    Task<ResponseModel> GetByIdAsync(int id);
    Task<ResponseModel> CreateAsync(CreateTicketTypeDto dto);
    Task<ResponseModel> UpdateAsync(int id, UpdateTicketTypeDto dto);
    Task<ResponseModel> ApproveOrRejectAsync(int id, ApprovalTicketTypeDto dto);
    Task<ResponseModel> DeleteAsync(int id);
}