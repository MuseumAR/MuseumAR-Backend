using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Category
{
    public class CategoryRepository : GenericRepository<Entities.Category>, ICategoryRepository
    {
        public CategoryRepository(MuseumAudioGuideContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Entities.Category>> GetCategoriesWithTranslationsAsync(int? museumId = null)
        {
            IQueryable<Entities.Category> query = _dbSet.Include(c => c.CategoryTranslations);
            if (museumId.HasValue)
            {
                query = query.Where(c => c.MuseumId == museumId.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<Entities.Category?> GetCategoryWithTranslationsByIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.CategoryTranslations)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }
    }
}
