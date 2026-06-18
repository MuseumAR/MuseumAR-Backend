using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; }
}
