using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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

        public async Task<ResponseModel> GetTicketTypeByIdAsync(int museumId, int ticketTypeId)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to view this ticket type.");

            var dto = _mapper.Map<TicketTypeDto>(ticketType);
            return ResponseModel.Success("Retrieve ticket type detail successfully.", dto);
        }

        public async Task<ResponseModel> UpdateTicketTypeAsync(int museumId, int ticketTypeId, UpdateTicketTypeDto updateDto)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to update this ticket type.");

            _mapper.Map(updateDto, ticketType);
            ticketType.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.TicketTypes.Update(ticketType);
            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<TicketTypeDto>(ticketType);
            return ResponseModel.Success("Ticket type updated successfully.", dto);
        }

        public async Task<ResponseModel> DeleteTicketTypeAsync(int museumId, int ticketTypeId)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to delete this ticket type.");

            // Check if any sold tickets reference this ticketType
            var hasPurchasedTickets = await _unitOfWork.Tickets.ExistsAsync(t => t.TicketTypeId == ticketTypeId);
            if (hasPurchasedTickets)
            {
                // Soft delete by deactivating it
                ticketType.IsActive = false;
                ticketType.UpdatedAt = System.DateTime.UtcNow;
                _unitOfWork.TicketTypes.Update(ticketType);
                await _unitOfWork.CompleteAsync();
                return ResponseModel.Success("Ticket type has purchased tickets, so it was deactivated instead of deleted.");
            }

            _unitOfWork.TicketTypes.Delete(ticketType);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Ticket type deleted successfully.");
        }

        public async Task<ResponseModel> CreateTicketPromotionAsync(int museumId, int ticketTypeId, CreateTicketPromotionDto dto)
        {
            var ticketType = await _unitOfWork.TicketTypes.GetByIdAsync(ticketTypeId);
            if (ticketType == null)
                return ResponseModel.NotFound("Ticket type not found.");

            if (ticketType.MuseumId != museumId)
                return ResponseModel.Forbidden("You are not authorized to manage promotions for this ticket type.");

            var today = DateTime.UtcNow.AddHours(7).Date;

            if (dto.StartDate.Date < today)
                return ResponseModel.BadRequest("Ngày bắt đầu không được ở trong quá khứ (Start date cannot be in the past).");

            if (dto.EndDate.Date < today)
                return ResponseModel.BadRequest("Ngày kết thúc không được ở trong quá khứ (End date cannot be in the past).");

            if (dto.StartDate >= dto.EndDate)
                return ResponseModel.BadRequest("Ngày kết thúc phải diễn ra sau ngày bắt đầu (End date must be after start date).");

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

            var today = DateTime.UtcNow.AddHours(7).Date;

            // Nếu người dùng thay đổi ngày bắt đầu, không cho chọn ngày trong quá khứ
            bool isStartDateChanged = promotion.StartDate.Date != dto.StartDate.Date;
            if (isStartDateChanged && dto.StartDate.Date < today)
                return ResponseModel.BadRequest("Ngày bắt đầu không được ở trong quá khứ (Start date cannot be in the past).");

            if (dto.EndDate.Date < today)
                return ResponseModel.BadRequest("Ngày kết thúc không được ở trong quá khứ (End date cannot be in the past).");

            if (dto.StartDate >= dto.EndDate)
                return ResponseModel.BadRequest("Ngày kết thúc phải diễn ra sau ngày bắt đầu (End date must be after start date).");

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

        public async Task<ResponseModel> GetRefundRequestsByMuseumAsync(int museumId, string? status = null)
        {
            var query = _unitOfWork.Context.TicketRefundRequests
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.TicketType)
                .Include(r => r.Visitor)
                    .ThenInclude(v => v.User)
                .Where(r => r.Ticket.TicketType.MuseumId == museumId);

            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = _mapper.Map<IEnumerable<TicketRefundRequestDto>>(requests);
            return ResponseModel.Success("Lấy danh sách yêu cầu hoàn vé thành công.", dtos);
        }

        public async Task<ResponseModel> ProcessRefundRequestAsync(int museumId, int refundRequestId, ProcessTicketRefundRequestDto dto)
        {
            var refundRequest = await _unitOfWork.Context.TicketRefundRequests
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.TicketType)
                .Include(r => r.Ticket)
                    .ThenInclude(t => t.Transaction)
                .FirstOrDefaultAsync(r => r.Id == refundRequestId);

            if (refundRequest == null)
            {
                return ResponseModel.NotFound("Không tìm thấy yêu cầu hoàn tiền này.");
            }

            if (refundRequest.Ticket.TicketType.MuseumId != museumId)
            {
                return ResponseModel.Forbidden("Bạn không có quyền xử lý yêu cầu hoàn tiền của bảo tàng khác.");
            }

            if (refundRequest.Status != "Pending")
            {
                return ResponseModel.BadRequest($"Yêu cầu này đã được xử lý trước đó với trạng thái '{refundRequest.Status}'.");
            }

            var now = DateTime.UtcNow;

            if (dto.IsApproved)
            {
                refundRequest.Status = "Approved";
                refundRequest.ProcessedAt = now;
                refundRequest.Ticket.Status = "Refunded";
                refundRequest.Ticket.UpdatedAt = now;

                // Nếu tất cả các vé trong cùng giao dịch đều đã hoàn tiền (hoặc đơn hàng chỉ có 1 vé), cập nhật trạng thái Transaction thành Refunded
                if (refundRequest.Ticket.TransactionId.HasValue)
                {
                    var transactionId = refundRequest.Ticket.TransactionId.Value;
                    var otherTickets = await _unitOfWork.Context.Tickets
                        .Where(t => t.TransactionId == transactionId && t.Id != refundRequest.TicketId)
                        .ToListAsync();

                    bool allRefunded = otherTickets.All(t => t.Status == "Refunded");
                    if (allRefunded && refundRequest.Ticket.Transaction != null)
                    {
                        refundRequest.Ticket.Transaction.PaymentStatus = "Refunded";
                        refundRequest.Ticket.Transaction.UpdatedAt = now;
                    }
                }
            }
            else
            {
                refundRequest.Status = "Rejected";
                refundRequest.RejectReason = !string.IsNullOrWhiteSpace(dto.RejectReason) ? dto.RejectReason.Trim() : "Ban quản lý từ chối yêu cầu hoàn tiền.";
                refundRequest.ProcessedAt = now;
                refundRequest.Ticket.Status = "Paid";
                refundRequest.Ticket.UpdatedAt = now;
            }

            _unitOfWork.TicketRefundRequests.Update(refundRequest);
            _unitOfWork.Tickets.Update(refundRequest.Ticket);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success(
                dto.IsApproved ? "Đã duyệt hoàn tiền thành công! Trạng thái vé đã được cập nhật thành Hoàn tiền." : "Đã từ chối yêu cầu hoàn tiền thành công.",
                new { refundRequestId = refundRequest.Id, status = refundRequest.Status }
            );
        }

        public async Task<ResponseModel> GetRevenueAnalyticsAsync(int museumId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _unitOfWork.Context.Tickets
                .Include(t => t.TicketType)
                .Where(t => t.TicketType.MuseumId == museumId &&
                            (t.Status == "Paid" || t.Status == "Used" || t.Status == "Refunded"));

            if (fromDate.HasValue)
            {
                var fromUtc = fromDate.Value.Date;
                query = query.Where(t => t.PurchaseDate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = toDate.Value.Date.AddDays(1);
                query = query.Where(t => t.PurchaseDate < toUtc);
            }

            var tickets = await query.ToListAsync();

            decimal totalGross = tickets.Sum(t => t.Price > 0 ? t.Price : (t.TicketType?.Price ?? 0));
            var refundedTickets = tickets.Where(t => t.Status == "Refunded").ToList();
            decimal totalRefunded = refundedTickets.Sum(t => t.Price > 0 ? t.Price : (t.TicketType?.Price ?? 0));
            decimal netRevenue = totalGross - totalRefunded;

            int totalSold = tickets.Count;
            int totalUsed = tickets.Count(t => t.Status == "Used");
            int totalRefundedCount = refundedTickets.Count;

            // Daily sales breakdown
            var dailySales = tickets
                .GroupBy(t => t.PurchaseDate.AddHours(7).Date)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    decimal dayGross = g.Sum(t => t.Price > 0 ? t.Price : (t.TicketType?.Price ?? 0));
                    decimal dayRefund = g.Where(t => t.Status == "Refunded").Sum(t => t.Price > 0 ? t.Price : (t.TicketType?.Price ?? 0));
                    return new DailySalesDto
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        GrossRevenue = dayGross,
                        RefundedAmount = dayRefund,
                        NetRevenue = dayGross - dayRefund,
                        TicketsSold = g.Count()
                    };
                })
                .ToList();

            // Revenue by Ticket Type breakdown
            var revenueByType = tickets
                .GroupBy(t => new { t.TicketTypeId, Name = t.TicketType?.Name ?? "Vé tham quan" })
                .Select(g =>
                {
                    decimal typeGross = g.Sum(t => t.Price > 0 ? t.Price : (t.TicketType?.Price ?? 0));
                    decimal typeRefund = g.Where(t => t.Status == "Refunded").Sum(t => t.Price > 0 ? t.Price : (t.TicketType?.Price ?? 0));
                    return new RevenueByTicketTypeDto
                    {
                        TicketTypeId = g.Key.TicketTypeId,
                        TicketTypeName = g.Key.Name,
                        TicketsSold = g.Count(),
                        Revenue = typeGross - typeRefund
                    };
                })
                .OrderByDescending(r => r.Revenue)
                .ToList();

            var result = new RevenueAnalyticsDto
            {
                TotalGrossRevenue = totalGross,
                TotalRefundedAmount = totalRefunded,
                NetRevenue = netRevenue,
                TotalTicketsSold = totalSold,
                TotalUsedTickets = totalUsed,
                TotalRefundedTickets = totalRefundedCount,
                DailySales = dailySales,
                RevenueByTicketType = revenueByType
            };

            return ResponseModel.Success("Lấy báo cáo doanh thu thành công.", result);
        }

        public async Task<ResponseModel> GetVisitorTrafficAnalyticsAsync(int museumId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var baseQuery = _unitOfWork.Context.Tickets
                .Include(t => t.TicketType)
                .Where(t => t.TicketType.MuseumId == museumId &&
                            (t.Status == "Paid" || t.Status == "Used" || t.Status == "Refunded"));

            if (fromDate.HasValue)
            {
                var fromUtc = fromDate.Value.Date;
                baseQuery = baseQuery.Where(t => t.PurchaseDate >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = toDate.Value.Date.AddDays(1);
                baseQuery = baseQuery.Where(t => t.PurchaseDate < toUtc);
            }

            var allTickets = await baseQuery.ToListAsync();
            var usedTickets = allTickets.Where(t => t.Status == "Used").ToList();

            int totalSold = allTickets.Count;
            int totalAdmitted = usedTickets.Count;
            double attendanceRate = totalSold > 0 ? Math.Round((double)totalAdmitted / totalSold * 100.0, 1) : 0.0;

            // Daily footfall (lưu lượng khách vào theo ngày check-in)
            var dailyFootfall = usedTickets
                .GroupBy(t => t.UpdatedAt.AddHours(7).Date)
                .OrderBy(g => g.Key)
                .Select(g => new DailyFootfallDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    VisitorCount = g.Count()
                })
                .ToList();

            // Peak hours traffic (Khung giờ cao điểm: 08:00 - 18:00)
            var peakHours = new List<PeakHourTrafficDto>();
            for (int hour = 8; hour <= 18; hour++)
            {
                int count = usedTickets.Count(t => t.UpdatedAt.AddHours(7).Hour == hour);
                peakHours.Add(new PeakHourTrafficDto
                {
                    Hour = hour,
                    HourLabel = $"{hour:D2}:00 - {(hour + 1):D2}:00",
                    VisitorCount = count
                });
            }

            var result = new VisitorTrafficDto
            {
                TotalAdmittedVisitors = totalAdmitted,
                TotalTicketsSold = totalSold,
                AttendanceRate = attendanceRate,
                DailyFootfall = dailyFootfall,
                PeakHoursTraffic = peakHours
            };

            return ResponseModel.Success("Lấy thống kê lưu lượng khách tham quan thành công.", result);
        }
    }
}