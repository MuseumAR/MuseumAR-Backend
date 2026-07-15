using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.TicketType;

public interface ITicketTypeRepository : IGenericRepository<Entities.TicketType>
{
    Task<IEnumerable<Entities.TicketType>> GetActiveTicketTypesAsync();
    Task<IEnumerable<Entities.TicketType>> GetTicketTypesByMuseumIdAsync(int museumId);
}
