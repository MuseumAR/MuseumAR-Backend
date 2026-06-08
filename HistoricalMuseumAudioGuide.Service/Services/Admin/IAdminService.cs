using HistoricalMuseumAudioGuide.Repository.Data.DTOs;
using HistoricalMuseumAudioGuide.Service.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin
{
    public interface IAdminService
    {
        // Museum Management
        Task<ResponseModel> GetAllMuseumsAsync();
        Task<ResponseModel> GetMuseumByIdAsync(int id);
        Task<ResponseModel> CreateMuseumAsync(CreateMuseumDto museumDto);
        Task<ResponseModel> UpdateMuseumAsync(int id, MuseumDto museumDto);
        Task<ResponseModel> DeleteMuseumAsync(int id);

        // Exhibit Management
        Task<ResponseModel> GetExhibitsByMuseumIdAsync(int museumId);
        Task<ResponseModel> CreateExhibitAsync(CreateExhibitDto exhibitDto);

        // Exhibition Management
        Task<ResponseModel> GetExhibitionsByMuseumIdAsync(int museumId);
        Task<ResponseModel> CreateExhibitionAsync(CreateExhibitionDto exhibitionDto);
    }
}
