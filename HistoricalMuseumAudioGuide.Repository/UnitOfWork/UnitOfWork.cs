using HistoricalMuseumAudioGuide.Repository.Data.Context;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Repositories.Analytics;
using HistoricalMuseumAudioGuide.Repository.Repositories.Category;
using HistoricalMuseumAudioGuide.Repository.Repositories.ContentVersion;
using HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitArasset;
using HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitImage;
using HistoricalMuseumAudioGuide.Repository.Repositories.ExhibitTranslation;
using HistoricalMuseumAudioGuide.Repository.Repositories.Museum;
using HistoricalMuseumAudioGuide.Repository.Repositories.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Repositories.Role;
using HistoricalMuseumAudioGuide.Repository.Repositories.Ticket;
using HistoricalMuseumAudioGuide.Repository.Repositories.TicketType;
using HistoricalMuseumAudioGuide.Repository.Repositories.Transaction;
using HistoricalMuseumAudioGuide.Repository.Repositories.User;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MuseumAudioGuideContext _context;

        public UnitOfWork(MuseumAudioGuideContext context)
        {
            _context = context;
            Analytics = new AnalyticsRepository(_context);
            Museums = new MuseumRepository(_context);
            Exhibits = new ExhibitRepository(_context);
            Categories = new CategoryRepository(_context);
            ExhibitTranslations = new ExhibitTranslationRepository(_context);
            ContentVersions = new ContentVersionRepository(_context);
            Users = new UserRepository(_context);
            Exhibitions = new GenericRepository<Exhibition>(_context);
            Roles = new RoleRepository(_context);
            ExhibitArassets = new ExhibitArassetRepository(_context);
            ExhibitImages = new ExhibitImageRepository(_context);
            OfflinePackages = new OfflinePackageRepository(_context);
            TicketTypes = new TicketTypeRepository(_context);
            Tickets = new TicketRepository(_context);
            Transactions = new TransactionRepository(_context);
        }
        public IAnalyticsRepository Analytics { get; private set; }
        public IMuseumRepository Museums { get; private set; }
        public IExhibitRepository Exhibits { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public IExhibitTranslationRepository ExhibitTranslations { get; private set; }
        public IContentVersionRepository ContentVersions { get; private set; }
        public IUserRepository Users { get; private set; }
        public IGenericRepository<Exhibition> Exhibitions { get; private set; }
        public IRoleRepository Roles { get; private set; }
        public IExhibitArassetRepository ExhibitArassets { get; private set; }
        public IExhibitImageRepository ExhibitImages { get; private set; }
        public IOfflinePackageRepository OfflinePackages { get; private set; }
        public ITicketTypeRepository TicketTypes { get; private set; }
        public ITicketRepository Tickets { get; private set; }
        public ITransactionRepository Transactions { get; private set; }

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
