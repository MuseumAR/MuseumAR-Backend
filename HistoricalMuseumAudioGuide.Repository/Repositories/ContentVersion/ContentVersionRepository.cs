using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Entities;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ContentVersion
{
    public class ContentVersionRepository : GenericRepository<Entities.ContentVersion>, IContentVersionRepository
    {
        public ContentVersionRepository(MuseumAudioGuideContext context) : base(context)
        {
        }
    }
}
