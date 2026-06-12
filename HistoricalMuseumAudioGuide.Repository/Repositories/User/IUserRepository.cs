using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.User;

public interface IUserRepository : IGenericRepository<Entities.User>
{
    Task<Entities.User?> GetUserByEmailAsync(string email);
    Task<Entities.User?> GetByResetTokenAsync(string token);
}
