using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Entities;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Category
{
    public interface ICategoryRepository : IGenericRepository<Entities.Category>
    {
        Task<System.Collections.Generic.IEnumerable<Entities.Category>> GetCategoriesWithTranslationsAsync(int? museumId = null);
        Task<Entities.Category?> GetCategoryWithTranslationsByIdAsync(int categoryId);
    }
}
