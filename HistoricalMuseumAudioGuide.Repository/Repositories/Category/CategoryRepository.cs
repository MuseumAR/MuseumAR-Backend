using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Entities;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Category
{
    public class CategoryRepository : GenericRepository<Entities.Category>, ICategoryRepository
    {
        public CategoryRepository(MuseumAudioGuideContext context) : base(context)
        {
        }
    }
}
