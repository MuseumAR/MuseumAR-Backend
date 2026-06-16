using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Ticket;

public class TicketRepository : GenericRepository<Entities.Ticket>, ITicketRepository
{
    public TicketRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entities.Ticket>> GetTicketsByVisitorIdAsync(int visitorId)
    {
        return await _dbSet.Include(t => t.TicketType).Where(t => t.VisitorId == visitorId).ToListAsync();
    }
}
