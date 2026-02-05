using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using QuizPlatform.Application.Interfaces;
using QuizPlatform.Application.Settings;

namespace QuizPlatform.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit for SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Mã OTP đặt lại mật khẩu - Anh ngữ Ephata";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2563eb;'>Anh ngữ Ephata</h2>
                        <p>Xin chào,</p>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu. Mã OTP của bạn là:</p>
                        <div style='background-color: #f1f5f9; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #1e40af;'>{otp}</span>
                        </div>
                        <p>Mã này sẽ hết hạn sau <strong>5 phút</strong>.</p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                        <p style='color: #64748b; font-size: 12px;'>Email này được gửi tự động, vui lòng không trả lời.</p>
                    </div>
                "
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
