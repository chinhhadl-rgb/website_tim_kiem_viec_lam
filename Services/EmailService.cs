using System.Net;
using System.Net.Mail;

namespace RecruitmentSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            try
            {
                var smtpServer = _configuration["Smtp:Server"] ?? "smtp.gmail.com";
                var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
                var senderEmail = _configuration["Smtp:SenderEmail"] ?? "";
                var senderName = _configuration["Smtp:SenderName"] ?? "Hệ thống Tuyển dụng";
                var username = _configuration["Smtp:Username"] ?? "";
                var password = _configuration["Smtp:Password"] ?? "";

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Chưa cấu hình tài khoản Email SMTP trong appsettings.json. Mã OTP chưa thể gửi qua Email.");
                    return;
                }

                using (var client = new SmtpClient(smtpServer, port))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = enableSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = bodyHtml,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Đã gửi email OTP thành công tới {Email}", toEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email tới {Email}", toEmail);
            }
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose = "xác thực tài khoản")
        {
            string subject = $"[{otpCode}] Mã xác thực OTP của bạn - Hệ thống Tuyển dụng";
            string bodyHtml = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; max-width: 600px; margin: auto; border: 1px solid #e0e0e0; border-radius: 10px;'>
                    <h2 style='color: #2a5298; text-align: center;'>MÃ XÁC THỰC OTP</h2>
                    <p>Xin chào <strong>{toEmail}</strong>,</p>
                    <p>Bạn (hoặc ai đó) vừa yêu cầu mã xác thực OTP để <strong>{purpose}</strong> trên Hệ thống Tuyển dụng.</p>
                    <div style='background-color: #f4f6f9; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #1e3c72;'>{otpCode}</span>
                    </div>
                    <p style='color: #d9534f;'>* Mã OTP này có hiệu lực trong <strong>5 phút</strong>. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #888; text-align: center;'>Trân trọng,<br/>Đội ngũ Hệ thống Tuyển dụng</p>
                </div>";

            await SendEmailAsync(toEmail, subject, bodyHtml);
        }
    }
}
