using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Repositories.Museum;
using HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MuseumAudioGuideContext _context;

        public UnitOfWork(MuseumAudioGuideContext context)
        {
            _context = context;
            Museums = new MuseumRepository(_context);
            Exhibits = new ExhibitRepository(_context);
            Exhibitions = new GenericRepository<Exhibition>(_context);
            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
        }

        public IMuseumRepository Museums { get; private set; }
        public IExhibitRepository Exhibits { get; private set; }
        public IGenericRepository<Exhibition> Exhibitions { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Role> Roles { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
