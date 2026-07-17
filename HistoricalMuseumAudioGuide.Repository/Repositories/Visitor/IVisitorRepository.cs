using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Visitor;

public interface IVisitorRepository : IGenericRepository<Entities.Visitor>
{
    Task<Entities.Visitor?> GetVisitorByEmailAsync(string email);
    Task<Entities.Visitor?> GetVisitorByUserIdAsync(int userId);
    Task<Entities.Visitor?> GetVisitorByDeviceIdAsync(string deviceId);
}
