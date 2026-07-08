using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services
{
    public interface IMuseumResolver
    {
        /// <summary>
        /// Returns the ID of the single museum configured in the system.
        /// Caches the result for the lifetime of the scoped service.
        /// </summary>
        Task<int> GetMuseumIdAsync();
    }

    public class MuseumResolver : IMuseumResolver
    {
        private readonly IUnitOfWork _unitOfWork;
        private int? _cachedId;

        public MuseumResolver(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetMuseumIdAsync()
        {
            if (_cachedId.HasValue) return _cachedId.Value;

            var museums = await _unitOfWork.Museums.GetAllAsync();
            var museum = museums.FirstOrDefault();

            if (museum == null)
                throw new InvalidOperationException("No museum is configured in the system. Please seed the database with a museum record.");

            _cachedId = museum.Id;
            return _cachedId.Value;
        }
    }
}
