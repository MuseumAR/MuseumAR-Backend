using System.ComponentModel.DataAnnotations;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
    public string Otp { get; set; } = null!;

    public string? OldPassword { get; set; }

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
