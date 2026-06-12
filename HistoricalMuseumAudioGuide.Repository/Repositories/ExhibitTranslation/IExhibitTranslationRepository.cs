using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitTranslation;

public interface IExhibitTranslationRepository : IGenericRepository<Entities.ExhibitTranslation>
{
    Task<Entities.ExhibitTranslation?> GetTranslationAsync(int exhibitId, string languageCode);
    Task<IEnumerable<Entities.ExhibitTranslation>> GetTranslationsByExhibitIdAsync(int exhibitId);
}
