using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin.Content;

public class MediaService : IMediaService
{
    private readonly IHostEnvironment _environment;
    private readonly string _baseFolder = "wwwroot";

    public MediaService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string subDirectory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        string uploadsFolder = Path.Combine(_environment.ContentRootPath, _baseFolder, "uploads", subDirectory);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Return relative URL for web access
        return $"/uploads/{subDirectory}/{uniqueFileName}";
    }

    public bool DeleteFile(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;
            
            // Remove leading slash if exists
            string relativePath = fileUrl.TrimStart('/');
            string fullPath = Path.Combine(_environment.ContentRootPath, _baseFolder, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
