using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class ChangePasswordRequestDto
{
    public string? OldPassword { get; set; }

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
