using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Visitor;

public class VisitorRepository : GenericRepository<Entities.Visitor>, IVisitorRepository
{
    public VisitorRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<Entities.Visitor?> GetVisitorByEmailAsync(string email)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(v => v.Email == email);
    }

    public async Task<Entities.Visitor?> GetVisitorByUserIdAsync(int userId)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(v => v.UserId == userId);
    }

    public async Task<Entities.Visitor?> GetVisitorByDeviceIdAsync(string deviceId)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(v => v.DeviceId == deviceId);
    }

    public async Task<Entities.Visitor?> GetVisitorWithUserByIdAsync(int id)
    {
        return await _dbSet.AsNoTracking().Include(v => v.User).FirstOrDefaultAsync(v => v.Id == id);
    }
}
