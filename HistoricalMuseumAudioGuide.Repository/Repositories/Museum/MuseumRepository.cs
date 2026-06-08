using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Museum
{
    public class MuseumRepository : GenericRepository<HistoricalMuseumAudioGuide.Repository.Entities.Museum>, IMuseumRepository
    {
        public MuseumRepository(MuseumAudioGuideContext context) : base(context)
        {
        }
    }
}
