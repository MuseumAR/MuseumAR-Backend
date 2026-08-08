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

    public async Task<IEnumerable<Entities.Ticket>> GetTicketsByTransactionIdAsync(int transactionId)
    {
        return await _dbSet.Where(t => t.TransactionId == transactionId).ToListAsync();
    }

    public async Task<Entities.Ticket?> GetTicketDetailByIdAsync(int id, int visitorId)
    {
        return await _dbSet
            .Include(t => t.TicketType)
                .ThenInclude(tt => tt.Museum)
            .Include(t => t.TicketType)
                .ThenInclude(tt => tt.Exhibition)
                    .ThenInclude(e => e!.ExhibitionTranslations)
            .Include(t => t.Transaction)
                .ThenInclude(tr => tr!.PaymentMethod)
            .FirstOrDefaultAsync(t => t.Id == id && t.VisitorId == visitorId);
    }

    public async Task<Entities.Ticket?> GetTicketByCodeAsync(string ticketCode)
    {
        return await _dbSet
            .Include(t => t.TicketType)
            .Include(t => t.Visitor)
                .ThenInclude(v => v.User)
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
    }
}
