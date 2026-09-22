namespace RecruitmentSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
        Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose = "xác thực tài khoản");
    }
}
