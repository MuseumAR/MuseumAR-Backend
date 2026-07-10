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
using HistoricalMuseumAudioGuide.Repository.Repositories.Visitor;
using System;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAnalyticsRepository Analytics { get; }
        IMuseumRepository Museums { get; }
        IExhibitRepository Exhibits { get; }
        ICategoryRepository Categories { get; }
        IExhibitTranslationRepository ExhibitTranslations { get; }
        IContentVersionRepository ContentVersions { get; }
        IUserRepository Users { get; }
        IVisitorRepository Visitors { get; }
        IGenericRepository<Exhibition> Exhibitions { get; }
        IRoleRepository Roles { get; }
        IExhibitArassetRepository ExhibitArassets { get; }
        IExhibitImageRepository ExhibitImages { get; }
        IOfflinePackageRepository OfflinePackages { get; }
        ITicketTypeRepository TicketTypes { get; }
        ITicketRepository Tickets { get; }
        ITransactionRepository Transactions { get; }
        IGenericRepository<Bookmark> Bookmarks { get; }
        IGenericRepository<VisitedExhibit> VisitedExhibits { get; }
        IGenericRepository<MuseumMap> MuseumMaps { get; }
        IGenericRepository<TourRoute> TourRoutes { get; }
        IGenericRepository<AnalyticsLog> AnalyticsLogs { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }
        IGenericRepository<SystemConfiguration> SystemConfigurations { get; }
        IGenericRepository<Theme> Themes { get; }
        IGenericRepository<AgeGroup> AgeGroups { get; }
        IGenericRepository<ExhibitMetadatum> ExhibitMetadata { get; }
        IGenericRepository<TagGroup> TagGroups { get; }
        IGenericRepository<Tag> Tags { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }
        
        Task<int> CompleteAsync();
    }
}
