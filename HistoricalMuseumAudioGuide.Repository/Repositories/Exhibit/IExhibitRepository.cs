using HistoricalMuseumAudioGuide.Repository.Interfaces;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit
{
    public interface IExhibitRepository : IGenericRepository<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit>
    {
        Task<System.Collections.Generic.IEnumerable<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit>> GetExhibitsWithTranslationsAndMetadataAsync(int museumId);
    }
}
