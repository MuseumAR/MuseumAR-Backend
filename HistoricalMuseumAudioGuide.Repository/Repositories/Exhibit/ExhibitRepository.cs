using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit
{
    public class ExhibitRepository : GenericRepository<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit>, IExhibitRepository
    {
        public ExhibitRepository(MuseumAudioGuideContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Entities.Exhibit>> GetExhibitsWithTranslationsAndMetadataAsync(int museumId)
        {
            return await _dbSet
                .Include(e => e.ExhibitTranslations)
                .Include(e => e.ExhibitMetadatum)
                .Where(e => e.MuseumId == museumId)
                .ToListAsync();
        }
    }
}
