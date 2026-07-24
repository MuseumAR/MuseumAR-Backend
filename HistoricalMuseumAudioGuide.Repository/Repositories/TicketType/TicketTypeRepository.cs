using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.TicketType;

public class TicketTypeRepository : GenericRepository<Entities.TicketType>, ITicketTypeRepository
{
    public TicketTypeRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entities.TicketType>> GetActiveTicketTypesAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive && t.Status == "Approved")
            .ToListAsync();
    }

    public async Task<IEnumerable<Entities.TicketType>> GetTicketTypesByMuseumIdAsync(int museumId, bool activeOnly = true)
    {
        var query = _dbSet.Where(t => t.MuseumId == museumId);

        if (activeOnly)
        {
            query = query.Where(t => t.IsActive && t.Status == "Approved");
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Entities.TicketType>> GetPendingTicketTypesAsync()
    {
        return await _dbSet
            .Where(t => t.Status == "Pending")
            .ToListAsync();
    }
}