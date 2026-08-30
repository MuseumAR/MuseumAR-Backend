using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;

public class CreateExhibitArassetDto
{
    [Required]
    public int ExhibitId { get; set; }

    [Required]
    [MaxLength(500)]
    public string AssetUrl { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string AssetType { get; set; } = "Model3D"; // Model3D, MarkerImage, etc.

    [MaxLength(255)]
    public string? Description { get; set; }
}
