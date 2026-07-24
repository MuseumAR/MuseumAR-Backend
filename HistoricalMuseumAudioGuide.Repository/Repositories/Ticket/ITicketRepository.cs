using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Interfaces;

public interface ITicketRepository : IGenericRepository<Ticket>
{
    // Thêm danh sách vé vừa tạo thuộc về một đơn hàng
    Task AddRangeAsync(IEnumerable<Ticket> tickets);

    // Lấy danh sách vé chi tiết thuộc một Transaction
    Task<IEnumerable<Ticket>> GetTicketsByTransactionIdAsync(int transactionId);
}