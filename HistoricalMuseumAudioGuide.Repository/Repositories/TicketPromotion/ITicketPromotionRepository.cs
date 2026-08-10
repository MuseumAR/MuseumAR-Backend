using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.TicketPromotion;

public interface ITicketPromotionRepository : IGenericRepository<Entities.TicketPromotion>
{
    /// <summary>
    /// Lấy tất cả promotions đang active của một danh sách ticket types (phục vụ batch mapping)
    /// </summary>
    Task<IEnumerable<Entities.TicketPromotion>> GetActivePromotionsForTicketTypesAsync(IEnumerable<int> ticketTypeIds);

    /// <summary>
    /// Lấy tất cả promotions của 1 ticket type
    /// </summary>
    Task<IEnumerable<Entities.TicketPromotion>> GetPromotionsByTicketTypeIdAsync(int ticketTypeId);

    /// <summary>
    /// Tìm promotion active theo ID và TicketTypeId để áp dụng khi tạo order
    /// </summary>
    Task<Entities.TicketPromotion?> GetActivePromotionByIdAndTicketTypeIdAsync(int promotionId, int ticketTypeId);
}
