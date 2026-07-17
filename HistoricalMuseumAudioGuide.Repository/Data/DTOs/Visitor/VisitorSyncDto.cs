using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;

public class VisitorSyncDto
{
    [Required]
    public string DeviceId { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string PreferredLang { get; set; } = "vi";

    public string? DeviceType { get; set; }

    public string? DeviceModel { get; set; }

    public string? AppVersion { get; set; }
}
