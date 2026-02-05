namespace QuizPlatform.Application.Interfaces;

/// <summary>
/// Service interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an OTP email for password reset.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="otp">The 6-digit OTP code</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendOtpEmailAsync(string toEmail, string otp);
}
