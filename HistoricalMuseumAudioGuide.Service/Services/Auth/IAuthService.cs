using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;

namespace HistoricalMuseumAudioGuide.Service.Services.Auth;

public interface IAuthService
{
    Task<ResponseModel> LoginAsync(LoginRequestDto request);
    Task<ResponseModel> LoginWithGoogleAsync(string idToken);
    Task<ResponseModel> RegisterAsync(RegisterRequestDto request);
    Task<ResponseModel> ForgotPasswordAsync(string email);
    Task<ResponseModel> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<ResponseModel> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
    Task<ResponseModel> SendPasswordOtpAsync(int userId);
    Task<ResponseModel> CheckHasPasswordAsync(int userId);
    Task<ResponseModel> RefreshTokenAsync(string refreshToken);
    Task<ResponseModel> VerifyEmailAsync(VerifyEmailRequestDto request);
    Task<ResponseModel> ResendVerificationEmailAsync(ResendVerificationRequestDto request);
}
