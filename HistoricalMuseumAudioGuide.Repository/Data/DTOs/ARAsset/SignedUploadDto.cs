using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset
{
    /// <summary>
    /// Request DTO for signing a Cloudinary upload.
    /// BE validates fileSize against maxBytes before returning signature.
    /// </summary>
    public class SignUploadRequestDto
    {
        [Required]
        public string FileName { get; set; } = null!;

        [Required]
        [Range(1, long.MaxValue)]
        public long FileSize { get; set; }

        public string? ContentType { get; set; }
    }

    /// <summary>
    /// Response DTO containing Cloudinary signature + upload URL.
    /// FE posts the file directly to Cloudinary using these params.
    /// </summary>
    public class SignUploadResponseDto
    {
        public string CloudName { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public long Timestamp { get; set; }
        public string Signature { get; set; } = null!;
        public string Folder { get; set; } = null!;
        public string PublicId { get; set; } = null!;
        public string UploadUrl { get; set; } = null!;
        public long MaxBytes { get; set; }
    }

    /// <summary>
    /// FE sends this after successful Cloudinary upload to persist the asset in DB.
    /// </summary>
    public class ConfirmUploadDto
    {
        [Required]
        public string PublicId { get; set; } = null!;

        [Required]
        public string SecureUrl { get; set; } = null!;

        [Range(1, long.MaxValue)]
        public long Bytes { get; set; }

        public string AssetType { get; set; } = "Model3D";
    }
}
