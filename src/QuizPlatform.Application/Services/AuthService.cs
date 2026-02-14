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
        // 1. Sanitize input
        request.Email = request.Email?.Trim()!;
        request.Username = request.Username?.Trim()!;

        // 2. Validate inputs
        if (!IsValidEmail(request.Email))
        {
            throw new InvalidOperationException("Email không hợp lệ");
        }

        if (!IsValidUsername(request.Username))
        {
            throw new InvalidOperationException("Tên đăng nhập không hợp lệ (3-20 ký tự, chỉ chứa chữ, số, dấu chấm và gạch dưới)");
        }

        if (!IsValidPassword(request.Password))
        {
            throw new InvalidOperationException("Mật khẩu phải có hơn 6 ký tự");
        }

        if (!request.DateOfBirth.HasValue)
        {
             throw new InvalidOperationException("Vui lòng nhập ngày sinh");
        }

        if (!IsValidDateOfBirth(request.DateOfBirth.Value))
        {
            throw new InvalidOperationException("Ngày sinh không hợp lệ (phải trên 6 tuổi)");
        }

        // Check if user already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email!))
        {
            throw new InvalidOperationException("Email đã được đăng ký");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username!))
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username!,
            Email = request.Email!,
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
        var token = _jwtService.GenerateToken(user.Id, user.Email!, user.Role.ToString());

        return new AuthResponse
        {
            Token = token,
            User = MapToUserDto(user)
        };
    }

    private bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            // Standard email regex
            var regex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        // Alphanumeric, dot, underscore, 3-20 chars
        var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._]{3,20}$");
        return regex.IsMatch(username);
    }

    private bool IsValidPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return password.Length > 6;
    }

    private bool IsValidDateOfBirth(DateTime dateOfBirth)
    {
        // Check if age is reasonable (e.g., > 6 years old)
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;

        return age >= 6;
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

#pragma warning disable CS1998
    public async Task<AuthResponse?> GoogleLoginAsync(string googleToken)
    {
#pragma warning restore CS1998
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
