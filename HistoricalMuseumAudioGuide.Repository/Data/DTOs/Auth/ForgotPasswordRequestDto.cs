using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
