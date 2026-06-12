using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Role;

public interface IRoleRepository : IGenericRepository<Entities.Role>
{
    Task<Entities.Role?> GetRoleByNameAsync(string roleName);
}
