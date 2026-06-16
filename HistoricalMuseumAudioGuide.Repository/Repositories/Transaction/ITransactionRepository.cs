using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Transaction;

public interface ITransactionRepository : IGenericRepository<Entities.Transaction>
{
    Task<Entities.Transaction?> GetByOrderCodeAsync(string orderCode);
}
