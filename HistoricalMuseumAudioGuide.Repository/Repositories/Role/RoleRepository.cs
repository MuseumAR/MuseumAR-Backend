using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Role;

public class RoleRepository : GenericRepository<Entities.Role>, IRoleRepository
{
    public RoleRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<Entities.Role?> GetRoleByNameAsync(string roleName)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.RoleName == roleName);
    }
}
