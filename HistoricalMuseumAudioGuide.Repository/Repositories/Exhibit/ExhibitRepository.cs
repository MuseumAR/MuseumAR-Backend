using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Interfaces;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit
{
    public class ExhibitRepository : GenericRepository<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit>, IExhibitRepository
    {
        public ExhibitRepository(MuseumAudioGuideContext context) : base(context)
        {
        }
    }
}
