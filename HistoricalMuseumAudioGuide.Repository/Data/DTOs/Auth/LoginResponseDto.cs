namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

public class LoginResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    // Optionally add refresh token here later if required by the client
}
