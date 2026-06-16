using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Ticket;

public interface ITicketRepository : IGenericRepository<Entities.Ticket>
{
    Task<IEnumerable<Entities.Ticket>> GetTicketsByVisitorIdAsync(int visitorId);
}
