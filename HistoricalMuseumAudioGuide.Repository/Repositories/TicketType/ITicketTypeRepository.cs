using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.TicketType;

public interface ITicketTypeRepository : IGenericRepository<Entities.TicketType>
{
    // Lấy danh sách vé đã Approved và IsActive = true (cho Visitor)
    Task<IEnumerable<Entities.TicketType>> GetActiveTicketTypesAsync();

    // Lấy danh sách vé theo MuseumId
    Task<IEnumerable<Entities.TicketType>> GetTicketTypesByMuseumIdAsync(int museumId, bool activeOnly = true);

    // Lấy các vé đang chờ duyệt theo MuseumId (cho Museum Manager)
    Task<IEnumerable<Entities.TicketType>> GetPendingTicketTypesAsync();
}