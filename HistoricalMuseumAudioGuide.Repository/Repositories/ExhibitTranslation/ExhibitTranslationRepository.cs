using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitTranslation;

public class ExhibitTranslationRepository : GenericRepository<Entities.ExhibitTranslation>, IExhibitTranslationRepository
{
    public ExhibitTranslationRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<Entities.ExhibitTranslation?> GetTranslationAsync(int exhibitId, string languageCode)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.ExhibitId == exhibitId && t.LanguageCode == languageCode);
    }

    public async Task<IEnumerable<Entities.ExhibitTranslation>> GetTranslationsByExhibitIdAsync(int exhibitId)
    {
        return await _dbSet.Where(t => t.ExhibitId == exhibitId).ToListAsync();
    }
}
