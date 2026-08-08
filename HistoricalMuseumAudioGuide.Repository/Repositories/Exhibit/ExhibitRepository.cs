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
                .Include(e => e.Map)
                .Include(e => e.Room)
                .Where(e => e.MuseumId == museumId)
                .ToListAsync();
        }

        public async Task<Entities.Exhibit?> GetExhibitByQrDataAsync(string qrData)
        {
            if (string.IsNullOrWhiteSpace(qrData)) return null;

            string cleanQr = qrData.Trim();
            int.TryParse(cleanQr, out int exhibitId);

            return await _dbSet
                .Include(e => e.ExhibitTranslations)
                .Include(e => e.ExhibitImages)
                .Include(e => e.ExhibitArassets)
                .Include(e => e.Category)
                    .ThenInclude(c => c!.CategoryTranslations)
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e =>
                    e.QrcodeData == cleanQr ||
                    e.ExhibitCode == cleanQr ||
                    (exhibitId > 0 && e.Id == exhibitId));
        }
    }
}
