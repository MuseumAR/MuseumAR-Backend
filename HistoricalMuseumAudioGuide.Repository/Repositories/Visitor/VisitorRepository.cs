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
        return await _dbSet.FirstOrDefaultAsync(v => v.Email == email);
    }
}
