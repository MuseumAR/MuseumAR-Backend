using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.User;

public class UserRepository : GenericRepository<Entities.User>, IUserRepository
{
    public UserRepository(MuseumAudioGuideContext context) : base(context)
    {
    }

    public async Task<Entities.User?> GetUserByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Entities.User?> GetByResetTokenAsync(string token)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.ResetTokenExpiresAt > DateTime.UtcNow);
    }
}
