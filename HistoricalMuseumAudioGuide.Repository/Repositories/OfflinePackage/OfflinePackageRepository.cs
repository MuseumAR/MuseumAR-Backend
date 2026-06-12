using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.OfflinePackage;

public class OfflinePackageRepository : GenericRepository<Entities.OfflinePackage>, IOfflinePackageRepository
{
    public OfflinePackageRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entities.OfflinePackage>> GetPackagesByMuseumIdAsync(int museumId)
    {
        return await _dbSet.Where(p => p.MuseumId == museumId).ToListAsync();
    }
}
