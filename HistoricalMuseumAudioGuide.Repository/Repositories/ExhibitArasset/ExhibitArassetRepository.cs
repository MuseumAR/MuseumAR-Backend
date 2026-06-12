using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitArasset;

public class ExhibitArassetRepository : GenericRepository<Entities.ExhibitArasset>, IExhibitArassetRepository
{
    public ExhibitArassetRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entities.ExhibitArasset>> GetArAssetsByExhibitIdAsync(int exhibitId)
    {
        return await _dbSet.Where(a => a.ExhibitId == exhibitId).ToListAsync();
    }
}
