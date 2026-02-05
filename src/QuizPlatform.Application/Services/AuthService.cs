using QuizPlatform.Application.DTOs.Auth;
using QuizPlatform.Application.DTOs.Password;
using QuizPlatform.Application.DTOs.Profile;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;
using BCrypt.Net;

namespace QuizPlatform.Application.Services;

/// <summary>
/// Service for authentication operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IEmailService emailService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            throw new InvalidOperationException("Username already taken");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            IsActive = true,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Generate token
        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is deactivated");
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponse?> GoogleLoginAsync(string googleToken)
    {
        // TODO: Implement Google OAuth validation
        throw new NotImplementedException("Google OAuth not yet implemented");
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToUserDto(user);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Return true to prevent email enumeration attacks
            return true;
        }

        // Generate 6-digit OTP
        var otp = new Random().Next(100000, 999999).ToString();
        
        // Set OTP expiry to 5 minutes
        user.PasswordResetOtp = otp;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user.Id, user);

        // Send OTP via email
        await _emailService.SendOtpEmailAsync(request.Email, otp);

        return true;
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return false;
        }

        // Check if OTP matches and is not expired
        if (user.PasswordResetOtp != request.Otp || 
            user.OtpExpiryTime == null || 
            user.OtpExpiryTime < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return false;
        }

        // Verify OTP again for security
        if (user.PasswordResetOtp != request.Otp || 
            user.OtpExpiryTime == null || 
            user.OtpExpiryTime < DateTime.UtcNow)
        {
            return false;
        }

        // Update password and clear OTP
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetOtp = null;
        user.OtpExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user.Id, user);

        return true;
    }

    public async Task<UserDto?> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Username))
        {
            // Check if username is taken by another user
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new InvalidOperationException("Username already taken");
            }
            user.Username = request.Username;
        }

        if (request.Gender.HasValue)
        {
            user.Gender = request.Gender.Value;
        }

        if (request.DateOfBirth.HasValue)
        {
            user.DateOfBirth = request.DateOfBirth.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user.Id, user);

        return MapToUserDto(user);
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user.Id, user);

        return true;
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Gender = user.Gender?.ToString(),
            DateOfBirth = user.DateOfBirth
        };
    }
}
