using AutoMapper;
using Google.Apis.Auth;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using HistoricalMuseumAudioGuide.Service.Services.Audit;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HistoricalMuseumAudioGuide.Repository.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IAuditService auditService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<ResponseModel> LoginAsync(LoginRequestDto request)
    {
        // Get user by email including Role
        var user = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);

        if (user == null || user.Status != "Active")
        {
            return ResponseModel.Unauthorized("Invalid credentials or account is inactive.");
        }

        // Verify Password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return ResponseModel.Unauthorized("Invalid credentials.");
        }

        // Update Last Login
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        // Generate Token
        var token = GenerateJwtToken(user);

        var responseDto = _mapper.Map<LoginResponseDto>(user);
        responseDto.AccessToken = token;

        return ResponseModel.Success("Login successful.", responseDto);
    }

    public async Task<ResponseModel> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ResponseModel.Conflict("Email already exists.");
        }

        var visitorRole = await _unitOfWork.Roles.GetRoleByNameAsync("Visitor");

        if (visitorRole == null)
        {
            return ResponseModel.Error("System Error: Default role 'Visitor' not found.");
        }

        var user = _mapper.Map<User>(request);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.RoleId = visitorRole.Id;
        user.Status = "Active";
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        await _auditService.LogActionAsync(
            userId: user.Id, 
            action: "AssignRole", 
            entityType: "User", 
            newValues: $"Assigned role 'Visitor' to new user {user.Email}", 
            ipAddress: "System", // Ideally get from HttpContext
            userAgent: "System"
        );

        return ResponseModel.Success("User registered successfully.", user.Id);
    }

    public async Task<ResponseModel> ForgotPasswordAsync(string email)
    {
        var user = await _unitOfWork.Users.GetUserByEmailAsync(email);
        if (user == null)
        {
            return ResponseModel.Success("If the email exists, a reset link has been sent.");
        }

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.ResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        Console.WriteLine($"[MOCK EMAIL] To: {email}, Reset Token: {user.PasswordResetToken}");
        return ResponseModel.Success("Reset link sent successfully.");
    }

    public async Task<ResponseModel> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByResetTokenAsync(request.Token);

        if (user == null)
        {
            return ResponseModel.BadRequest("Invalid or expired reset token.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Password has been reset successfully.");
    }

    public async Task<ResponseModel> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return ResponseModel.NotFound("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
        {
            return ResponseModel.BadRequest("Incorrect old password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Password changed successfully.");
    }

    public async Task<ResponseModel> LoginWithGoogleAsync(string idToken)
    {
        try
        {
            var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? _configuration["Google:ClientId"];
            
            if (string.IsNullOrEmpty(clientId))
            {
                return ResponseModel.Error("System Error: Google Client ID is not configured.");
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { clientId }
            };

            // Verify Google Token
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // Find user in DB
            var user = await _unitOfWork.Users.GetUserByEmailAsync(payload.Email);

            if (user == null)
            {
                // Get Visitor Role
                var visitorRole = await _unitOfWork.Roles.GetRoleByNameAsync("Visitor");

                if (visitorRole == null)
                {
                    return ResponseModel.Error("System Error: Default role 'Visitor' not found.");
                }

                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    PasswordHash = "GOOGLE_OAUTH_USER", // No password for Google users
                    RoleId = visitorRole.Id,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();
                
                // Re-fetch to get includes
                user = await _unitOfWork.Users.GetUserByEmailAsync(payload.Email);
            }

            if (user!.Status != "Active")
            {
                return ResponseModel.Unauthorized("Account is inactive.");
            }

            // Update Last Login
            user.LastLoginAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            // Generate Local JWT
            var token = GenerateJwtToken(user);

            var responseDto = _mapper.Map<LoginResponseDto>(user);
            responseDto.AccessToken = token;

            return ResponseModel.Success("Google login successful.", responseDto);
        }
        catch (InvalidJwtException)
        {
            return ResponseModel.Unauthorized("Invalid Google Token.");
        }
        catch (Exception ex)
        {
            return ResponseModel.Error("An error occurred during Google login: " + ex.Message);
        }
    }

    private string GenerateJwtToken(User user)
    {
        // 1. Create Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Visitor")
        };

        // 2. Add MuseumId to claims if user belongs to a museum
        if (user.MuseumId.HasValue)
        {
            var museumIdStr = user.MuseumId.Value.ToString();
            claims.Add(new Claim("MuseumId", museumIdStr));
        }

        // 3. Create Key and Credentials
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                     ?? _configuration["Jwt:Secret"] 
                     ?? throw new InvalidOperationException("JWT Secret is not configured.");
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];

        // 4. Create Token Descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Token valid for 1 day
            SigningCredentials = creds,
            Issuer = issuer,
            Audience = audience
        };

        // 5. Generate and Write Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
