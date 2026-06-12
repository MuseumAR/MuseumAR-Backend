using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitImage;

public interface IExhibitImageRepository : IGenericRepository<Entities.ExhibitImage>
{
    Task<IEnumerable<Entities.ExhibitImage>> GetImagesByExhibitIdAsync(int exhibitId);
}
