using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitArasset;

public interface IExhibitArassetRepository : IGenericRepository<Entities.ExhibitArasset>
{
    Task<IEnumerable<Entities.ExhibitArasset>> GetArAssetsByExhibitIdAsync(int exhibitId);
}
