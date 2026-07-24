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

    public async Task AddRangeAsync(IEnumerable<Entities.Ticket> tickets)
    {
        await _context.Tickets.AddRangeAsync(tickets);
    }

    public async Task<IEnumerable<Entities.Ticket>> GetTicketsByTransactionIdAsync(int transactionId)
    {
        return await _context.Tickets
            .Include(t => t.TicketType)
            .Where(t => t.TransactionId == transactionId)
            .ToListAsync();
    }
}