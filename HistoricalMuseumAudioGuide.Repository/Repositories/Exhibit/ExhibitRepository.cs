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
            return await _dbSet.AsNoTracking()
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

            return await _dbSet.AsNoTracking()
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

        public async Task<(IEnumerable<Entities.Exhibit> Items, int TotalCount)> GetExhibitsPagedAsync(
            int museumId, int page, int pageSize, bool includeUnpublished, string? search, string? status)
        {
            var query = _dbSet.AsNoTracking()
                .Include(e => e.ExhibitTranslations)
                .Include(e => e.ExhibitArassets)
                .Include(e => e.Room)
                .Include(e => e.Map)
                .Where(e => e.MuseumId == museumId);

            if (!includeUnpublished)
            {
                query = query.Where(e => e.Status == "Published");
            }
            else if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                if (s.Equals("Published", System.StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Status == "Published");
                }
                else if (s.Equals("Draft", System.StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Status == "Draft");
                }
                // If unrecognized status (e.g. 'all'), do not filter — do not fail
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(e =>
                    (e.ExhibitCode != null && e.ExhibitCode.ToLower().Contains(term)) ||
                    e.ExhibitTranslations.Any(t => t.Title != null && t.Title.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.SortOrder)
                .ThenByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Entities.Exhibit?> GetExhibitByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var trimmed = code.Trim().ToLower();

            return await _dbSet.AsNoTracking()
                .Include(e => e.ExhibitTranslations)
                .Include(e => e.ExhibitMetadatum)
                .Include(e => e.ExhibitArassets)
                .Include(e => e.Map)
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.ExhibitCode != null && e.ExhibitCode.ToLower() == trimmed);
        }
    }
}

