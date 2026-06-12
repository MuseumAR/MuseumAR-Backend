using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class GoogleLoginRequestDto
{
    [Required]
    public string IdToken { get; set; } = null!;
}
