using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitImage;

public class ExhibitImageRepository : GenericRepository<Entities.ExhibitImage>, IExhibitImageRepository
{
    public ExhibitImageRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Entities.ExhibitImage>> GetImagesByExhibitIdAsync(int exhibitId)
    {
        return await _dbSet.Where(i => i.ExhibitId == exhibitId).ToListAsync();
    }
}
