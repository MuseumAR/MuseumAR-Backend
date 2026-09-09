using HistoricalMuseumAudioGuide.Repository.Interfaces;

namespace HistoricalMuseumAudioGuide.Repository.Repositories.Exhibit
{
    public interface IExhibitRepository : IGenericRepository<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit>
    {
        Task<System.Collections.Generic.IEnumerable<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit>> GetExhibitsWithTranslationsAndMetadataAsync(int museumId);
        Task<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit?> GetExhibitByQrDataAsync(string qrData);

        /// <summary>
        /// Paged query with includes for list DTO projection (ExhibitArassets, ExhibitTranslations, Room, Map).
        /// Returns (items, totalCount) for building PagedResultDto.
        /// </summary>
        Task<(System.Collections.Generic.IEnumerable<HistoricalMuseumAudioGuide.Repository.Entities.Exhibit> Items, int TotalCount)>
            GetExhibitsPagedAsync(int museumId, int page, int pageSize, bool includeUnpublished, string? search, string? status);
    }
}
