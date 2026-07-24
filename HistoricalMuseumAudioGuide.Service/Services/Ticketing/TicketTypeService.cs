using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Ticketing;

public class TicketTypeService : ITicketTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TicketTypeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    private static DateTime GetVietnamTime() => DateTime.UtcNow.AddHours(7);

    public async Task<ResponseModel> GetTicketTypesAsync(int? museumId, bool activeOnly = true)
    {
        IEnumerable<TicketType> list;

        if (museumId.HasValue)
        {
            list = await _unitOfWork.TicketTypes.GetTicketTypesByMuseumIdAsync(museumId.Value, activeOnly);
        }
        else
        {
            list = activeOnly
                ? await _unitOfWork.TicketTypes.GetActiveTicketTypesAsync()
                : await _unitOfWork.TicketTypes.GetAllAsync();
        }

        var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(list);
        return ResponseModel.Success("Get ticket types successfully", dtos);
    }
    public async Task<ResponseModel> GetAllForCMSAsync(int? museumId = null)
    {
        IEnumerable<TicketType> list;

        if (museumId.HasValue)
        {
            var allList = await _unitOfWork.TicketTypes.GetAllAsync();
            list = allList.Where(t => t.MuseumId == museumId.Value);
        }
        else
        {
            list = await _unitOfWork.TicketTypes.GetAllAsync();
        }

        var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(list);
        return ResponseModel.Success("Get all ticket types for admin successfully", dtos);
    }

    public async Task<ResponseModel> GetPendingTicketTypesAsync()
    {
        var list = await _unitOfWork.TicketTypes.GetPendingTicketTypesAsync();
        var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(list);
        return ResponseModel.Success("Get pending ticket types successfully", dtos);
    }

    public async Task<ResponseModel> GetByIdAsync(int id)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
        if (ticketType == null)
            return ResponseModel.NotFound("Ticket type not found.");

        var dto = _mapper.Map<TicketTypeDto>(ticketType);
        return ResponseModel.Success("Get ticket type successfully", dto);
    }

    // Content Manager tạo vé mới
    public async Task<ResponseModel> CreateAsync(CreateTicketTypeDto dto)
    {
        // 1. Kiểm tra Museum có tồn tại không
        var museum = await _unitOfWork.Museums.GetByIdAsync(dto.MuseumId);
        if (museum == null)
            return ResponseModel.NotFound($"Museum with ID {dto.MuseumId} does not exist.");

        // 2. Kiểm tra Exhibition có tồn tại không (nếu DTO có truyền ExhibitionId)
        if (dto.ExhibitionId.HasValue)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetByIdAsync(dto.ExhibitionId.Value);
            if (exhibition == null)
                return ResponseModel.NotFound($"Exhibition with ID {dto.ExhibitionId.Value} does not exist.");

            // (Optional) Kiểm tra xem Exhibition đó có thuộc Bảo tàng này không
            if (exhibition.MuseumId != dto.MuseumId)
                return ResponseModel.BadRequest($"Exhibition with ID {dto.ExhibitionId.Value} does not belong to Museum {dto.MuseumId}.");
        }

        var ticketType = _mapper.Map<TicketType>(dto);

        var now = GetVietnamTime();
        ticketType.IsActive = true;
        ticketType.Status = "Pending";
        ticketType.CreatedAt = now;
        ticketType.UpdatedAt = now;

        await _unitOfWork.TicketTypes.AddAsync(ticketType);
        await _unitOfWork.CompleteAsync();

        var resultDto = _mapper.Map<TicketTypeDto>(ticketType);
        return ResponseModel.Success("Ticket type created successfully. Waiting for Museum Manager approval.", resultDto);
    }

    // Content Manager cập nhật loại vé
    public async Task<ResponseModel> UpdateAsync(int id, UpdateTicketTypeDto dto)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
        if (ticketType == null)
            return ResponseModel.NotFound("Ticket type not found.");

        // Kiểm tra Exhibition nếu có cập nhật ExhibitionId mới
        if (dto.ExhibitionId.HasValue)
        {
            var exhibition = await _unitOfWork.Exhibitions.GetByIdAsync(dto.ExhibitionId.Value);
            if (exhibition == null)
                return ResponseModel.NotFound($"Exhibition with ID {dto.ExhibitionId.Value} does not exist.");

            if (exhibition.MuseumId != ticketType.MuseumId)
                return ResponseModel.BadRequest($"Exhibition with ID {dto.ExhibitionId.Value} does not belong to Museum {ticketType.MuseumId}.");
        }

        _mapper.Map(dto, ticketType);

        ticketType.Status = "Pending"; // Sửa thông tin thì đưa về Pending chờ duyệt lại
        ticketType.UpdatedAt = GetVietnamTime();

        _unitOfWork.TicketTypes.Update(ticketType);
        await _unitOfWork.CompleteAsync();

        var resultDto = _mapper.Map<TicketTypeDto>(ticketType);
        return ResponseModel.Success("Ticket type updated successfully and reset to Pending.", resultDto);
    }

    // Museum Manager Duyệt / Từ chối
    public async Task<ResponseModel> ApproveOrRejectAsync(int id, ApprovalTicketTypeDto dto)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
        if (ticketType == null)
            return ResponseModel.NotFound("Ticket type not found.");

        ticketType.Status = dto.Status;
        ticketType.UpdatedAt = GetVietnamTime();

        _unitOfWork.TicketTypes.Update(ticketType);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success($"Ticket type has been {dto.Status.ToLower()} successfully");
    }

    // Content Manager xóa mềm
    public async Task<ResponseModel> DeleteAsync(int id)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(id);
        if (ticketType == null)
            return ResponseModel.NotFound("Ticket type not found.");

        ticketType.IsActive = false;
        ticketType.Status = "Rejected";
        ticketType.UpdatedAt = GetVietnamTime();

        _unitOfWork.TicketTypes.Update(ticketType);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Ticket type deactivated successfully");
    }
}