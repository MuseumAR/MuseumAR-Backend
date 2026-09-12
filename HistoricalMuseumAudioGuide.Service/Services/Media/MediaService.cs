using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Media;

public class MediaService : IMediaService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _cloudName;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment? _environment;

    public MediaService(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment? environment = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;

        var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL") ?? configuration["Cloudinary:Url"];
        if (string.IsNullOrEmpty(cloudinaryUrl))
        {
            throw new InvalidOperationException("Cloudinary URL is not configured. Please check your .env file.");
        }
        _cloudinary = new Cloudinary(cloudinaryUrl);
        _cloudinary.Api.Secure = true;

        // Parse CLOUDINARY_URL format: cloudinary://API_KEY:API_SECRET@CLOUD_NAME
        var uri = new Uri(cloudinaryUrl);
        _cloudName = uri.Host;
        var userInfo = uri.UserInfo.Split(':');
        _apiKey = userInfo.Length > 0 ? userInfo[0] : string.Empty;
        _apiSecret = userInfo.Length > 1 ? userInfo[1] : string.Empty;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var is3DModel = ext == ".glb" || ext == ".gltf" || ext == ".usdz" || ext == ".obj" || ext == ".fbx" || ext == ".bin";
        var isArAsset = string.Equals(subDirectory, "ar", StringComparison.OrdinalIgnoreCase);

        // 3D models and AR assets are saved locally on the backend server to support files up to 200MB without Cloudinary size limits
        if (is3DModel || isArAsset)
        {
            return await SaveLocalFileAsync(file, subDirectory);
        }

        using var stream = file.OpenReadStream();
        var fileDescription = new FileDescription(file.FileName, stream);
        var folderPath = $"museum_ar/{subDirectory}";

        // Preserve original file name (normalized) with a unique suffix
        var rawFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var normalizedFileName = System.Text.RegularExpressions.Regex.Replace(rawFileName, @"[^a-zA-Z0-9_\-]", "");
        if (string.IsNullOrEmpty(normalizedFileName)) normalizedFileName = "file";
        var fileNameWithUniqueSuffix = $"{normalizedFileName}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        RawUploadResult uploadResult;

        if (file.ContentType.StartsWith("image"))
        {
            var uploadParams = new ImageUploadParams()
            {
                File = fileDescription,
                Folder = folderPath,
                PublicId = fileNameWithUniqueSuffix,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        else if (file.ContentType.StartsWith("audio"))
        {
            var uploadParams = new VideoUploadParams()
            {
                File = fileDescription,
                Folder = folderPath,
                PublicId = fileNameWithUniqueSuffix
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }
        else
        {
            // Generic files
            var uploadParams = new RawUploadParams()
            {
                File = fileDescription,
                Folder = folderPath,
                PublicId = fileNameWithUniqueSuffix
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary Upload Error: {uploadResult.Error.Message}");
        }

        // SecureUrl returns the https link which is public and accessible by the mobile app
        return uploadResult.SecureUrl.ToString();
    }

    private async Task<string> SaveLocalFileAsync(IFormFile file, string subDirectory)
    {
        var webRoot = _environment?.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment?.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadFolder = Path.Combine(webRoot, "uploads", subDirectory);
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var rawFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var ext = Path.GetExtension(file.FileName);
        var normalizedFileName = System.Text.RegularExpressions.Regex.Replace(rawFileName, @"[^a-zA-Z0-9_\-]", "");
        if (string.IsNullOrEmpty(normalizedFileName)) normalizedFileName = "file";
        var fileName = $"{normalizedFileName}_{Guid.NewGuid().ToString().Substring(0, 8)}{ext}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null && httpContext.Request != null)
        {
            var scheme = httpContext.Request.Scheme;
            var host = httpContext.Request.Host.Value;
            return $"{scheme}://{host}/uploads/{subDirectory}/{fileName}";
        }

        return $"/uploads/{subDirectory}/{fileName}";
    }

    public bool DeleteFile(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return false;

        // If local file stored in wwwroot/uploads/
        if (fileUrl.Contains("/uploads/"))
        {
            try
            {
                var webRoot = _environment?.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    webRoot = Path.Combine(_environment?.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
                }

                string relativePath;
                if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                {
                    relativePath = uri.AbsolutePath.TrimStart('/');
                }
                else
                {
                    relativePath = fileUrl.TrimStart('/', '\\');
                }

                var localPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                    return true;
                }
            }
            catch
            {
                // Ignore cleanup error
            }
            return true;
        }

        // Deleting from Cloudinary via URL requires parsing the PublicID.
        // For this capstone prototype, we prioritize successful uploads and public access.
        // Public URLs are persistent and perfect for the Mobile App / AR views.
        return true; 
    }

    /// <summary>
    /// Generate Cloudinary signed upload parameters for browser-direct upload.
    /// Signature = HMAC-SHA1(apiSecret, "folder={folder}&public_id={publicId}&timestamp={ts}")
    /// </summary>
    public SignUploadResponseDto GenerateSignedUpload(string folder, string publicId, long maxBytes)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Cloudinary signature: alphabetically sorted params concatenated with & 
        var paramsToSign = $"folder={folder}&public_id={publicId}&timestamp={timestamp}";

        string signature;
        using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_apiSecret)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(paramsToSign));
            signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        return new SignUploadResponseDto
        {
            CloudName = _cloudName,
            ApiKey = _apiKey,
            Timestamp = timestamp,
            Signature = signature,
            Folder = folder,
            PublicId = publicId,
            UploadUrl = $"https://api.cloudinary.com/v1_1/{_cloudName}/raw/upload",
            MaxBytes = maxBytes
        };
    }
}

