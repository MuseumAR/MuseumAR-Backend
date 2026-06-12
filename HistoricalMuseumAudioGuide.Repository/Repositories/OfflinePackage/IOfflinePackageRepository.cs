using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.OfflinePackage;

public interface IOfflinePackageRepository : IGenericRepository<Entities.OfflinePackage>
{
    Task<IEnumerable<Entities.OfflinePackage>> GetPackagesByMuseumIdAsync(int museumId);
}
