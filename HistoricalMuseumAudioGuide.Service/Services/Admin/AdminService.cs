using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetAllMuseumsAsync()
        {
            var museums = await _unitOfWork.Museums.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<MuseumDto>>(museums);
            return ResponseModel.Success("Museums retrieved successfully", dtos);
        }

        public async Task<ResponseModel> GetMuseumByIdAsync(int id)
        {
            var museum = await _unitOfWork.Museums.GetByIdAsync(id);
            if (museum == null) return ResponseModel.NotFound("Museum not found");
            var dto = _mapper.Map<MuseumDto>(museum);
            return ResponseModel.Success("Museum retrieved successfully", dto);
        }

        public async Task<ResponseModel> CreateMuseumAsync(CreateMuseumDto createMuseumDto)
        {
            var museum = _mapper.Map<Museum>(createMuseumDto);
            await _unitOfWork.Museums.AddAsync(museum);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<MuseumDto>(museum);
            return ResponseModel.Success("Museum created successfully", dto);
        }

        public async Task<ResponseModel> UpdateMuseumAsync(int id, MuseumDto museumDto)
        {
            var museum = await _unitOfWork.Museums.GetByIdAsync(id);
            if (museum == null) return ResponseModel.NotFound("Museum not found");
            
            _mapper.Map(museumDto, museum);
            _unitOfWork.Museums.Update(museum);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Museum updated successfully");
        }

        public async Task<ResponseModel> DeleteMuseumAsync(int id)
        {
            var museum = await _unitOfWork.Museums.GetByIdAsync(id);
            if (museum == null) return ResponseModel.NotFound("Museum not found");
            
            _unitOfWork.Museums.Delete(museum);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Museum deleted successfully");
        }

        public async Task<ResponseModel> GetAllTicketTypesAsync()
        {
            var ticketTypes = await _unitOfWork.TicketTypes.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(ticketTypes);
            return ResponseModel.Success("Ticket types retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateTicketTypeAsync(CreateTicketTypeDto ticketTypeDto)
        {
            var ticketType = _mapper.Map<TicketType>(ticketTypeDto);
            ticketType.CreatedAt = System.DateTime.UtcNow;
            ticketType.UpdatedAt = System.DateTime.UtcNow;
            await _unitOfWork.TicketTypes.AddAsync(ticketType);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<TicketTypeDto>(ticketType);
            return ResponseModel.Success("Ticket type created successfully", dto);
        }
    }
}
