namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class VerifyEmailRequestDto
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}
