using HistoricalMuseumAudioGuide.Repository.Data.DTOs.SystemConfig;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.SystemConfig;

public interface ISystemConfigService
{
    Task<ResponseModel> GetAllConfigsAsync();
    Task<ResponseModel> GetConfigByKeyAsync(string key);
    Task<ResponseModel> UpdateConfigAsync(string key, UpdateSystemConfigDto dto, int? updatedBy);
}
