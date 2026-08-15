using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendTicketConfirmationEmailAsync(
        string toEmail,
        string visitorName,
        string orderCode,
        decimal totalAmount,
        int ticketCount,
        string ticketTypeName)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            Console.WriteLine("[EmailService Warning]: Recipient email is empty. Skipping email dispatch.");
            return;
        }

        string smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _configuration["SMTP_HOST"] ?? "smtp.gmail.com";
        string smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? _configuration["SMTP_PORT"] ?? "587";
        string smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? _configuration["SMTP_USER"] ?? "";
        string smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? _configuration["SMTP_PASS"] ?? "";
        string fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? _configuration["SMTP_FROM_NAME"] ?? "MuseumAR";

        if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            Console.WriteLine($"[EmailService Info]: SMTP credentials not configured (SMTP_USER/SMTP_PASS). Ticket email notification for {toEmail} was not sent via SMTP.");
            return;
        }

        if (!int.TryParse(smtpPortStr, out int smtpPort))
        {
            smtpPort = 587;
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(smtpUser, fromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = $"[MuseumAR] Mua vé thành công - Đơn hàng #{orderCode}";
            message.IsBodyHtml = true;

            string nowFormatted = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");
            string formattedAmount = string.Format("{0:N0}", totalAmount);

            message.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5e6c8; margin: 0; padding: 20px; color: #2b1d0e; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #fff8e7; border-radius: 16px; border: 1px solid rgba(200,155,60,0.3); padding: 32px; box-shadow: 0 4px 12px rgba(43,29,14,0.08); }}
        .header {{ text-align: center; border-bottom: 2px solid #c89b3c; padding-bottom: 20px; margin-bottom: 24px; }}
        .title {{ color: #c89b3c; font-size: 26px; font-weight: bold; margin: 0; }}
        .subtitle {{ color: #7d5a3c; font-size: 15px; margin-top: 6px; }}
        .content {{ font-size: 15px; line-height: 1.6; color: #2b1d0e; }}
        .ticket-box {{ background: #ffffff; border-radius: 12px; border: 1px dashed #c89b3c; padding: 20px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 32px; font-size: 12px; color: #a08060; border-top: 1px solid rgba(200,155,60,0.2); padding-top: 16px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1 class=""title"">🏛️ MuseumAR</h1>
            <div class=""subtitle"">Xác nhận mua vé tham quan thành công</div>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{WebUtility.HtmlEncode(visitorName)}</strong>,</p>
            <p>Cảm ơn bạn đã đặt vé tham quan tại <strong>MuseumAR - Bảo tàng Lịch sử</strong>. Đơn hàng mua vé của bạn đã được xác nhận thanh toán thành công!</p>
            
            <div class=""ticket-box"">
                <table width=""100%"" cellpadding=""6"" cellspacing=""0"" style=""border-collapse: collapse;"">
                    <tr>
                        <td style=""color: #7d5a3c; font-weight: 500;"">Mã đơn hàng:</td>
                        <td style=""text-align: right; font-weight: bold; font-family: monospace; color: #c89b3c;"">{WebUtility.HtmlEncode(orderCode)}</td>
                    </tr>
                    <tr>
                        <td style=""color: #7d5a3c; font-weight: 500;"">Loại vé:</td>
                        <td style=""text-align: right; font-weight: bold;"">{WebUtility.HtmlEncode(ticketTypeName)}</td>
                    </tr>
                    <tr>
                        <td style=""color: #7d5a3c; font-weight: 500;"">Số lượng:</td>
                        <td style=""text-align: right; font-weight: bold;"">{ticketCount} vé</td>
                    </tr>
                    <tr>
                        <td style=""color: #7d5a3c; font-weight: 500;"">Thời gian mua:</td>
                        <td style=""text-align: right; font-weight: bold;"">{nowFormatted}</td>
                    </tr>
                    <tr style=""border-top: 1px solid #f0e0c0;"">
                        <td style=""color: #a67c2d; font-weight: bold; font-size: 16px; padding-top: 12px;"">Tổng thanh toán:</td>
                        <td style=""text-align: right; color: #a67c2d; font-weight: bold; font-size: 16px; padding-top: 12px;"">{formattedAmount} VND</td>
                    </tr>
                </table>
            </div>

            <p style=""margin-top: 20px;"">
                💡 <strong>Hướng dẫn check-in:</strong> Bạn có thể mở ứng dụng di động hoặc trang web MuseumAR, truy cập phần <em>Vé của tôi</em> để xuất trình mã QR check-in khi đến cửa bảo tàng.
            </p>
        </div>
        <div class=""footer"">
            <p>Đây là email tự động từ hệ thống MuseumAR. Vui lòng không phản hồi trực tiếp email này.</p>
            <p>&copy; 2026 MuseumAR - Bảo tàng Lịch sử TP.HCM. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
            Console.WriteLine($"[EmailService]: Ticket confirmation email sent successfully to {toEmail} for order #{orderCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService Error]: Failed to send ticket confirmation email to {toEmail}: {ex.Message}");
        }
    }

    public async Task SendEmailVerificationAsync(string toEmail, string userName, string tokenCode)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            Console.WriteLine("[EmailService Warning]: Recipient email is empty. Skipping email verification dispatch.");
            return;
        }

        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _configuration["SMTP_HOST"] ?? _configuration["EmailSettings:SmtpServer"];
        var smtpPortStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? _configuration["SMTP_PORT"] ?? _configuration["EmailSettings:SmtpPort"];
        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? _configuration["SMTP_USER"] ?? _configuration["EmailSettings:Username"];
        var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? _configuration["SMTP_PASS"] ?? _configuration["EmailSettings:Password"];
        var fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _configuration["SMTP_FROM"] ?? _configuration["EmailSettings:FromEmail"] ?? smtpUser ?? "noreply@museumar.com";

        if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass) || string.IsNullOrWhiteSpace(smtpHost))
        {
            Console.WriteLine($"[EmailService Info]: SMTP credentials not fully configured. Email verification token [{tokenCode}] for {toEmail} recorded locally.");
            return;
        }

        int smtpPort = int.TryParse(smtpPortStr, out int p) ? p : 587;

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail, "Museum Audio Guide");
            message.To.Add(new MailAddress(toEmail));
            message.Subject = "Xác nhận địa chỉ Email - Museum AR";
            message.IsBodyHtml = true;

            message.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 20px; }}
        .container {{ max-width: 550px; background: #ffffff; margin: 0 auto; border-radius: 12px; padding: 30px; border: 1px solid #e0e0e0; }}
        .header {{ text-align: center; border-bottom: 2px solid #6366f1; padding-bottom: 15px; margin-bottom: 20px; }}
        .header h2 {{ color: #1e293b; margin: 0; font-size: 22px; }}
        .content {{ color: #334155; line-height: 1.6; font-size: 15px; }}
        .code-box {{ background: #f1f5f9; border-radius: 8px; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #4338ca; margin: 20px 0; border: 1px dashed #6366f1; }}
        .footer {{ text-align: center; color: #94a3b8; font-size: 12px; margin-top: 30px; border-top: 1px solid #f1f5f9; padding-top: 15px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>🏛️ Bảo tàng Lịch sử - Museum AR</h2>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{userName}</strong>,</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Museum AR</strong>. Để thực hiện mua vé tham quan, vui lòng nhập mã xác thực email bên dưới:</p>
            
            <div class=""code-box"">{tokenCode}</div>
            
            <p style=""font-size: 13px; color: #64748b;"">* Mã xác thực này có hiệu lực trong vòng 24 giờ. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
        </div>
        <div class=""footer"">
            <p>Email tự động từ Hệ thống Thuyết minh Bảo tàng Museum AR. Vui lòng không phản hồi email này.</p>
        </div>
    </div>
</body>
</html>";

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);
            Console.WriteLine($"[EmailService]: Verification email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService Error]: Failed to send verification email to {toEmail}: {ex.Message}");
        }
    }
}
