using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Transaction;

public interface ITransactionRepository : IGenericRepository<Entities.Transaction>
{
    // Lấy thông tin giao dịch kèm theo Mã đơn hàng (dùng khi check trạng thái / Webhook)
    Task<Entities.Transaction?> GetByOrderCodeAsync(string orderCode);
}