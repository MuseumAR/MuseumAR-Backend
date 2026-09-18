using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Transaction;

public class TransactionRepository : GenericRepository<Entities.Transaction>, ITransactionRepository
{
    public TransactionRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<Entities.Transaction?> GetByOrderCodeAsync(string orderCode)
    {
        return await _dbSet.Include(t => t.Tickets).FirstOrDefaultAsync(t => t.OrderCode == orderCode);
    }
}
