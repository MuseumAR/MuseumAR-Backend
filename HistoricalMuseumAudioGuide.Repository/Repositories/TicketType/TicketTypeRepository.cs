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
        return await _dbSet.Where(t => t.IsActive).ToListAsync();
    }
}
