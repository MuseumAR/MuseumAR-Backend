using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Email;

public interface IEmailService
{
    Task SendTicketConfirmationEmailAsync(
        string toEmail,
        string visitorName,
        string orderCode,
        decimal totalAmount,
        int ticketCount,
        string ticketTypeName);

    Task SendEmailVerificationAsync(string toEmail, string userName, string tokenCode);
}
