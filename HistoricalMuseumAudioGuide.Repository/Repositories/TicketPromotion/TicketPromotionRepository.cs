using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.TicketPromotion;

public class TicketPromotionRepository : GenericRepository<Entities.TicketPromotion>, ITicketPromotionRepository
{
    public TicketPromotionRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entities.TicketPromotion>> GetActivePromotionsForTicketTypesAsync(IEnumerable<int> ticketTypeIds)
    {
        var now = DateTime.UtcNow.AddHours(7);
        var ids = ticketTypeIds.ToList();
        return await _dbSet.AsNoTracking()
            .Where(p => ids.Contains(p.TicketTypeId)
                        && p.IsActive
                        && p.StartDate <= now
                        && p.EndDate >= now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.TicketPromotion>> GetPromotionsByTicketTypeIdAsync(int ticketTypeId)
    {
        return await _dbSet.AsNoTracking()
            .Where(p => p.TicketTypeId == ticketTypeId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Entities.TicketPromotion?> GetActivePromotionByIdAndTicketTypeIdAsync(int promotionId, int ticketTypeId)
    {
        var now = DateTime.UtcNow.AddHours(7);
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == promotionId
                                   && p.TicketTypeId == ticketTypeId
                                   && p.IsActive
                                   && p.StartDate <= now
                                   && p.EndDate >= now);
    }
}
