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

    public async Task<IEnumerable<Entities.OfflinePackage>> GetPackagesByMuseumIdAsync(int museumId, int? exhibitionId = null)
    {
        var query = _dbSet.AsNoTracking()
            .Include(p => p.Exhibition)
                .ThenInclude(e => e!.ExhibitionTranslations)
            .Include(p => p.OfflinePackageTranslations)
            .Where(p => p.MuseumId == museumId);

        if (exhibitionId.HasValue)
        {
            query = query.Where(p => p.ExhibitionId == exhibitionId.Value);
        }

        return await query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).ToListAsync();
    }
}
