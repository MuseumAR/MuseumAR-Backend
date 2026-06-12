using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin.Content;

public interface IMediaService
{
    Task<string> UploadFileAsync(IFormFile file, string subDirectory);
    bool DeleteFile(string fileUrl);
}
