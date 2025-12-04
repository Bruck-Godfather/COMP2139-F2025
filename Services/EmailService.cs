using MailKit.Net.Smtp;
using MimeKit;

namespace COMP2138_ICE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try 
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using (var client = new SmtpClient())
                {
                    // Accept all SSL certificates (for development only)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);

                    if (!string.IsNullOrEmpty(emailSettings["SenderEmail"]) && !string.IsNullOrEmpty(emailSettings["SenderPassword"]))
                    {
                        await client.AuthenticateAsync(emailSettings["SenderEmail"], emailSettings["SenderPassword"]);
                    }

                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the app
                // In a real app, you might want to throw or handle this differently
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
