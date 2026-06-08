using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Repositories.Museum;
using HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit;
using System;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMuseumRepository Museums { get; }
        IExhibitRepository Exhibits { get; }
        IGenericRepository<Exhibition> Exhibitions { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        
        Task<int> CompleteAsync();
    }
}
