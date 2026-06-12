using Microsoft.AspNetCore.Mvc;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HistoricalMuseumAudioGuide.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.LoginAsync(request);
        return ResponseParser.Result(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var response = await _authService.RegisterAsync(request);
        return ResponseParser.Result(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var response = await _authService.ForgotPasswordAsync(request.Email);
        return ResponseParser.Result(response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var response = await _authService.ResetPasswordAsync(request);
        return ResponseParser.Result(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        var response = await _authService.ChangePasswordAsync(userId, request);
        
        return ResponseParser.Result(response);
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authService.LoginWithGoogleAsync(request.IdToken);
        return ResponseParser.Result(response);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Since we are using stateless JWT, logout is primarily handled on the client side
        // by deleting the token. If we had a refresh token in the DB, we would revoke it here.
        return Ok(new { message = "Logged out successfully. Please remove your local token." });
    }
}
