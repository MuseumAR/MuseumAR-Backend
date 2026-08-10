using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Analytics
{
    public class MuseumManagerService : IMuseumManagerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MuseumManagerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel> GetMuseumDashboardDataAsync(int museumId)
        {
            // 1. Kiểm tra xem bảo tàng có tồn tại thực tế dưới DB không
            var museum = await _unitOfWork.Museums.GetByIdAsync(museumId);
            if (museum == null)
                return ResponseModel.NotFound($"Museum with ID {museumId} not found.");

            // 2. Gọi tuần tự các hàm thống kê từ Repository để tránh lỗi Concurrency trên DbContext
            var qrStats = await _unitOfWork.Analytics.GetQrScanStatsAsync(museumId);
            var popularExhibits = await _unitOfWork.Analytics.GetPopularExhibitsAsync(museumId, topCount: 5);
            var langStats = await _unitOfWork.Analytics.GetLanguageUsageStatsAsync(museumId);
            var offlineDownloads = await _unitOfWork.Analytics.GetTotalOfflineDownloadsAsync(museumId);

            // 3. Tính toán các chỉ số KPI tổng hợp (Summary Metrics) hiển thị trên đầu Dashboard
            // Tính tổng số lượt quét QR dựa trên tổng dữ liệu quét của từng hiện vật
            int totalQrScans = qrStats.Sum(x => x.ScanCount);

            // Tính thời lượng nghe Audio trung bình (Đổi từ Giây sang Phút để thân thiện với người xem)
            double avgListeningDurationMinutes = 0;
            if (popularExhibits.Any())
            {
                double avgSeconds = popularExhibits.Average(x => x.AvgDurationSeconds);
                avgListeningDurationMinutes = Math.Round(avgSeconds / 60.0, 2);
            }

            // 4. Đóng gói toàn bộ cấu trúc dữ liệu vào DTO tổng hợp
            var dashboardData = new MuseumDashboardDto
            {
                TotalQrScans = totalQrScans,
                AverageListeningDurationMinutes = avgListeningDurationMinutes,
                TotalOfflineDownloads = offlineDownloads,
                ExhibitScanStats = qrStats,
                PopularExhibits = popularExhibits,
                LanguageUsageStats = langStats
            };

            return ResponseModel.Success("Get museum dashboard analytics successfully.", dashboardData);
        }

        public async Task<ResponseModel> GetTicketTypesByMuseumAsync(int museumId)
        {
            var ticketTypes = await _unitOfWork.TicketTypes.GetTicketTypesByMuseumIdAsync(museumId);
            var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(ticketTypes);
            return ResponseModel.Success("Retrieve ticket types successfully", dtos);
        }

        public async Task<ResponseModel> CreateTicketTypeAsync(int museumId, CreateTicketTypeDto createDto)
        {
            var ticketType = _mapper.Map<TicketType>(createDto);
            ticketType.MuseumId = museumId;
            ticketType.Status = "Pending";
            ticketType.CreatedAt = System.DateTime.UtcNow;
            ticketType.UpdatedAt = System.DateTime.UtcNow;

            await _unitOfWork.TicketTypes.AddAsync(ticketType);
            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<TicketTypeDto>(ticketType);
            return ResponseModel.Success("Ticket type created successfully", dto);
        }

        public async Task<ResponseModel> PublishTicketTypeAsync(int museumId, int ticketTypeId)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to publish this ticket type.");

            ticketType.Status = "Approved";
            ticketType.IsActive = true;
            ticketType.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.TicketTypes.Update(ticketType);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Ticket type published successfully.");
        }

        public async Task<ResponseModel> CreateTicketPromotionAsync(int museumId, int ticketTypeId, CreateTicketPromotionDto dto)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to manage promotions for this ticket type.");

            if (dto.StartDate >= dto.EndDate)
                return ResponseModel.BadRequest("Start date must be before end date.");

            if (dto.DiscountValue <= 0)
                return ResponseModel.BadRequest("Discount value must be greater than 0.");

            if (dto.DiscountType == "Percentage" && dto.DiscountValue > 100)
                return ResponseModel.BadRequest("Percentage discount cannot exceed 100%.");

            if (dto.DiscountType == "FixedAmount" && dto.DiscountValue > ticketType.Price)
                return ResponseModel.BadRequest("Fixed amount discount cannot exceed ticket price.");

            var promotion = _mapper.Map<HistoricalMuseumAudioGuide.Repository.Entities.TicketPromotion>(dto);
            promotion.TicketTypeId = ticketTypeId;
            promotion.CreatedAt = System.DateTime.UtcNow;
            promotion.UpdatedAt = System.DateTime.UtcNow;

            await _unitOfWork.TicketPromotions.AddAsync(promotion);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<TicketPromotionDto>(promotion);
            return ResponseModel.Success("Ticket promotion created successfully.", resultDto);
        }

        public async Task<ResponseModel> GetTicketPromotionsByTicketTypeAsync(int museumId, int ticketTypeId)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to view promotions for this ticket type.");

            var promotions = await _unitOfWork.TicketPromotions.GetPromotionsByTicketTypeIdAsync(ticketTypeId);
            var dtos = _mapper.Map<IEnumerable<TicketPromotionDto>>(promotions);
            return ResponseModel.Success("Retrieve ticket promotions successfully.", dtos);
        }

        public async Task<ResponseModel> GetTicketPromotionByIdAsync(int museumId, int promotionId)
        {
            var promotion = await _unitOfWork.TicketPromotions.GetByIdAsync(promotionId);
            if (promotion == null)
                return ResponseModel.NotFound("Ticket promotion not found.");

            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(promotion.TicketTypeId);
            if (ticketType == null || ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to view this promotion.");

            var dto = _mapper.Map<TicketPromotionDto>(promotion);
            return ResponseModel.Success("Retrieve ticket promotion detail successfully.", dto);
        }

        public async Task<ResponseModel> UpdateTicketPromotionAsync(int museumId, int promotionId, UpdateTicketPromotionDto dto)
        {
            var promotion = await _unitOfWork.TicketPromotions.GetByIdAsync(promotionId);
            if (promotion == null)
                return ResponseModel.NotFound("Ticket promotion not found.");

            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(promotion.TicketTypeId);
            if (ticketType == null || ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to update this promotion.");

            if (dto.StartDate >= dto.EndDate)
                return ResponseModel.BadRequest("Start date must be before end date.");

            if (dto.DiscountValue <= 0)
                return ResponseModel.BadRequest("Discount value must be greater than 0.");

            if (dto.DiscountType == "Percentage" && dto.DiscountValue > 100)
                return ResponseModel.BadRequest("Percentage discount cannot exceed 100%.");

            if (dto.DiscountType == "FixedAmount" && dto.DiscountValue > ticketType.Price)
                return ResponseModel.BadRequest("Fixed amount discount cannot exceed ticket price.");

            _mapper.Map(dto, promotion);
            promotion.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.TicketPromotions.Update(promotion);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<TicketPromotionDto>(promotion);
            return ResponseModel.Success("Ticket promotion updated successfully.", resultDto);
        }

        public async Task<ResponseModel> DeleteTicketPromotionAsync(int museumId, int promotionId)
        {
            var promotion = await _unitOfWork.TicketPromotions.GetByIdAsync(promotionId);
            if (promotion == null)
                return ResponseModel.NotFound("Ticket promotion not found.");

            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(promotion.TicketTypeId);
            if (ticketType == null || ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to delete this promotion.");

            _unitOfWork.TicketPromotions.Delete(promotion);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Ticket promotion deleted successfully.");
        }

        public async Task<ResponseModel> ToggleTicketPromotionAsync(int museumId, int promotionId, bool isActive)
        {
            var promotion = await _unitOfWork.TicketPromotions.GetByIdAsync(promotionId);
            if (promotion == null)
                return ResponseModel.NotFound("Ticket promotion not found.");

            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(promotion.TicketTypeId);
            if (ticketType == null || ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to manage this promotion.");

            promotion.IsActive = isActive;
            promotion.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.TicketPromotions.Update(promotion);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success($"Ticket promotion {(isActive ? "activated" : "deactivated")} successfully.");
        }
    }
}