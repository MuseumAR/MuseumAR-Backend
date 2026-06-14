using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
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

        public async Task<ResponseModel> GetExhibitsByMuseumIdAsync(int museumId)
        {
            var exhibits = await _unitOfWork.Exhibits.FindAsync(e => e.MuseumId == museumId);
            var dtos = _mapper.Map<IEnumerable<ExhibitDto>>(exhibits);
            return ResponseModel.Success("Exhibits retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateExhibitAsync(CreateExhibitDto createExhibitDto)
        {
            var exhibit = _mapper.Map<Exhibit>(createExhibitDto);
            await _unitOfWork.Exhibits.AddAsync(exhibit);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<ExhibitDto>(exhibit);
            return ResponseModel.Success("Exhibit created successfully", dto);
        }

        public async Task<ResponseModel> GetExhibitionsByMuseumIdAsync(int museumId)
        {
            var exhibitions = await _unitOfWork.Exhibitions.FindAsync(e => e.MuseumId == museumId);
            var dtos = _mapper.Map<IEnumerable<ExhibitionDto>>(exhibitions);
            return ResponseModel.Success("Exhibitions retrieved successfully", dtos);
        }

        public async Task<ResponseModel> CreateExhibitionAsync(CreateExhibitionDto createExhibitionDto)
        {
            var exhibition = _mapper.Map<Exhibition>(createExhibitionDto);
            await _unitOfWork.Exhibitions.AddAsync(exhibition);
            await _unitOfWork.CompleteAsync();
            var dto = _mapper.Map<ExhibitionDto>(exhibition);
            return ResponseModel.Success("Exhibition created successfully", dto);
        }
    }
}
