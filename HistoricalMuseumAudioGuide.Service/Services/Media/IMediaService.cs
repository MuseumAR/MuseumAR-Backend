using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Media;

public interface IMediaService
{
    Task<string> UploadFileAsync(IFormFile file, string subDirectory);
    bool DeleteFile(string fileUrl);

    /// <summary>
    /// Generate Cloudinary signed upload parameters so the browser can upload directly.
    /// No file passes through the App Service.
    /// </summary>
    SignUploadResponseDto GenerateSignedUpload(string folder, string publicId, long maxBytes);
}
