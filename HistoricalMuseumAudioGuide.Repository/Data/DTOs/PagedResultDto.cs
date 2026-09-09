using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs
{
    /// <summary>
    /// Generic paged result DTO — same shape as audit log paging.
    /// Reusable for exhibits, rooms, and any future paged endpoints.
    /// </summary>
    public class PagedResultDto<T>
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
